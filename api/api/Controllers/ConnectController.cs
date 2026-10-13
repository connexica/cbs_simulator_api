using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ConnectController : ControllerBase
{
    // Individual Existing Customer verification test scenarios (2026-08-08). Minimal, additive
    // dispatch: for the 4 functions listed in TestScenarios.md, the incoming request's account
    // number/CIF (decoded from req.Data — Base64(JSON), same encoding
    // SignatureGeneratorExtension.EncodeObjectToBase64 already produces) is looked up in an optional
    // "<FunctionName>.scenarios.json" file. If found, that scenario's response is returned instead of
    // the default static file. If the file doesn't exist, is malformed, the key isn't found, or
    // req.Data can't be decoded for any reason, behavior falls back exactly to the pre-existing
    // static-file read — every other FunctionName branch below is completely untouched, and any
    // account number not listed in a scenarios file behaves exactly as before this change.
    private static readonly HashSet<string> ScenarioAwareFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "RetailAccountInquiry", "GetCifByAccountNumber", "RetailCIFInquiry", "AccountFreezeInquiry"
    };

    private static string? TryGetScenarioResponse(string functionName, string? base64Data)
    {
        try
        {
            var scenarioFile = $"{functionName}.scenarios.json";
            if (!System.IO.File.Exists(scenarioFile) || string.IsNullOrWhiteSpace(base64Data)) return null;

            var decodedJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Data));
            using var reqDoc = JsonDocument.Parse(decodedJson);

            string? key = functionName switch
            {
                // RetailAccountInquiryRequestDto: SBAcctInqRequest.SBAcctInqRq.SBAcctId.AcctId
                "RetailAccountInquiry" => reqDoc.RootElement
                    .TryGetProperty("SBAcctInqRequest", out var a) && a.TryGetProperty("SBAcctInqRq", out var b)
                    && b.TryGetProperty("SBAcctId", out var c) && c.TryGetProperty("AcctId", out var d)
                    ? d.GetString() : null,
                // GetCifByAccountNumberRequestDto: AccountNumber (flat)
                "GetCifByAccountNumber" => reqDoc.RootElement.TryGetProperty("AccountNumber", out var e)
                    ? e.GetString() : null,
                // RetCustInqRequestDto: RetCustInqRequest.RetCustInqRq.CustId (this is the CIF)
                "RetailCIFInquiry" => reqDoc.RootElement
                    .TryGetProperty("RetCustInqRequest", out var f) && f.TryGetProperty("RetCustInqRq", out var g)
                    && g.TryGetProperty("CustId", out var h) ? h.GetString() : null,
                // FreezeAcctsInqRequestDto: FreezeAcctsInqRequest.FreezeAcctsInqRq.AcctId
                "AccountFreezeInquiry" => reqDoc.RootElement
                    .TryGetProperty("FreezeAcctsInqRequest", out var i) && i.TryGetProperty("FreezeAcctsInqRq", out var j)
                    && j.TryGetProperty("AcctId", out var k) ? k.GetString() : null,
                _ => null
            };
            if (string.IsNullOrWhiteSpace(key)) return null;

            var scenarios = System.IO.File.ReadAllText(scenarioFile);
            using var scenarioDoc = JsonDocument.Parse(scenarios);
            if (!scenarioDoc.RootElement.TryGetProperty(key, out var match)) return null;

            return match.GetRawText();
        }
        catch
        {
            // Any decode/parse failure silently falls back to the existing default-file behavior —
            // never breaks an existing, already-working test flow.
            return null;
        }
    }

    // RetailCifOpening request check (2026-10-13). The simulator used to return the static success file
    // whatever was sent. It now reads the same request the Admin application sends to the real CBS
    // (RetCustAddRequest.RetCustAddRq -- CustDtls.CustData + RelatedDtls, same element names and casing; no
    // simulator-only field) and rejects only values that are present and wrong. Every element checked here is
    // optional: a request without it, an undecodable request, or the request shape sent before these fields
    // existed still gets the unchanged success response. Returns null when the request is acceptable.
    private static string? ValidateRetailCifOpening(string? base64Data)
    {
        // Tightened 2026-10-13: a request that DECODES but is not a RetailCifOpening request (not a JSON object,
        // or without RetCustAddRequest.RetCustAddRq / CustDtls.CustData) is rejected, not silently accepted.
        // Only a request with no data, or data that cannot be decoded at all, keeps the old static success.
        if (string.IsNullOrWhiteSpace(base64Data)) return null;
        string decoded;
        try
        {
            decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Data));
        }
        catch
        {
            return null;
        }

        JsonObject? rq;
        try
        {
            rq = (JsonNode.Parse(decoded) as JsonObject)?["RetCustAddRequest"]?["RetCustAddRq"] as JsonObject;
        }
        catch
        {
            return "RetailCifOpening request data is not valid JSON.";
        }
        if (rq == null) return "RetailCifOpening requires RetCustAddRequest.RetCustAddRq.";
        if (rq["CustDtls"]?["CustData"] is not JsonObject) return "RetailCifOpening requires CustDtls.CustData.";
        foreach (var section in new[] { "RelatedDtls" })
            if (rq[section] != null && rq[section] is not JsonObject) return $"{section} must be an object when supplied.";
        if ((rq["RelatedDtls"] as JsonObject)?["DemographicData"] is JsonNode demographicNode && demographicNode is not JsonObject)
            return "DemographicData must be an object when supplied.";

        static string? Text(JsonNode? node) => node is JsonValue v
            ? (v.TryGetValue<string>(out var s) ? s : v.ToJsonString().Trim('"'))
            : null;
        static IEnumerable<JsonObject> Items(JsonNode? node) => node switch
        {
            JsonArray array => array.OfType<JsonObject>(),
            JsonObject single => new[] { single },
            _ => Array.Empty<JsonObject>()
        };
        static bool IsDate(string value, out DateTime date) =>
            DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out date);

        try
        {
            // Mobile: PhoneNumLocalCode carries the actual mobile number (older requests sent "977").
            foreach (var contact in Items(rq["CustDtls"]?["CustData"]?["PhoneEmailDtls"]))
            {
                if (!string.Equals(Text(contact["PhoneOrEmail"]), "PHONE", StringComparison.OrdinalIgnoreCase)) continue;
                var localCode = Text(contact["PhoneNumLocalCode"]);
                if (localCode != null && (localCode.Length is < 3 or > 15 || !localCode.All(char.IsDigit)))
                    return "PhoneNumLocalCode must contain 3 to 15 digits.";
            }

            var related = rq["RelatedDtls"] as JsonObject;

            // Identity documents: any number of entries, none repeated; dates valid when supplied.
            // The same DocCode + ReferenceNum twice with DIFFERENT details is rejected. An exact repeat of an
            // entry is tolerated: callers that have not been changed (Non-Individual stakeholder CIF) still
            // send National ID front and back as two identical NATID entries and must keep succeeding.
            var seenDocuments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var document in Items(related?["EntityDoctData"]))
            {
                var docCode = Text(document["DocCode"]);
                if (string.IsNullOrWhiteSpace(docCode)) return "EntityDoctData.DocCode is required.";
                var documentKey = $"{docCode}|{Text(document["ReferenceNum"])?.Trim()}";
                var documentJson = document.ToJsonString();
                if (seenDocuments.TryGetValue(documentKey, out var firstJson))
                {
                    if (!string.Equals(firstJson, documentJson, StringComparison.Ordinal))
                        return $"Conflicting duplicate EntityDoctData entries for DocCode {docCode}.";
                }
                else
                {
                    seenDocuments[documentKey] = documentJson;
                }

                DateTime issueDate = default, expiryDate = default;
                var issueText = Text(document["IssueDt"]);
                var expiryText = Text(document["ExpDt"]);
                if (!string.IsNullOrWhiteSpace(issueText) && !IsDate(issueText, out issueDate))
                    return $"EntityDoctData.IssueDt is not a valid date for DocCode {docCode}.";
                if (!string.IsNullOrWhiteSpace(expiryText) && !IsDate(expiryText, out expiryDate))
                    return $"EntityDoctData.ExpDt is not a valid date for DocCode {docCode}.";
                if (issueDate != default && expiryDate != default && expiryDate <= issueDate)
                    return $"EntityDoctData.ExpDt must be later than IssueDt for DocCode {docCode}.";
            }

            if (related?["DemographicData"] is JsonObject demographic)
            {
                // CURRENT_EMPLOYMENT and EDU_DET may be sent together; each type once.
                var seenTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var misc in Items(demographic["DemographicMiscData"]))
                {
                    var type = Text(misc["Type"]);
                    if (string.IsNullOrWhiteSpace(type)) return "DemographicMiscData.Type is required.";
                    if (!seenTypes.Add(type)) return $"Duplicate DemographicMiscData entry for Type {type}.";
                }

                foreach (var incomeField in new[] { "Annual_Salary_Income", "Annual_Total_Income" })
                {
                    var amountText = Text(demographic[incomeField]);
                    if (amountText != null && (!decimal.TryParse(amountText, System.Globalization.NumberStyles.Number,
                            System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount < 0))
                        return $"{incomeField} must be a non-negative number.";
                }

                var incomeCurrency = Text(demographic["CU_Annual_Salary_Income"]);
                if (incomeCurrency != null && (incomeCurrency.Length != 3 || !incomeCurrency.All(char.IsLetter)))
                    return "CU_Annual_Salary_Income must be a 3-letter currency code.";

                foreach (var textField in new[] { "ResidenceCountry", "SourceOfIncome" })
                    if (demographic[textField] != null && string.IsNullOrWhiteSpace(Text(demographic[textField])))
                        return $"{textField} must not be empty when supplied.";

                foreach (var flagField in new[] { "DonotCallFlag", "DonotMailFlag" })
                {
                    var flag = Text(demographic[flagField]);
                    if (demographic[flagField] != null && flag is not ("Y" or "N"))
                        return $"{flagField} must be Y or N when supplied.";
                }
            }
        }
        catch
        {
            // OLD: return null (accepted). A decodable request whose fields cannot be read is malformed.
            return "RetailCifOpening request could not be validated: unexpected field structure.";
        }

        return null;
    }

    // Simulator state is process-local and is lost on restart. Publish immutable snapshots per CIF.
    private static readonly ConcurrentDictionary<string, JsonObject> RetCustModState = new(StringComparer.OrdinalIgnoreCase);

    private static object StoreRetCustMod(RequestDto req)
    {
        object Failure(string message) => new { Code = "1", Message = message, Data = (object?)null };
        try
        {
            if (!string.Equals(req.FunctionName, "RetCustModKYC", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(req.Data))
                return Failure("RetCustModKYC request data is required.");

            var root = JsonNode.Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(req.Data)));
            if (root is not JsonObject payload
                || payload["RetCustModRequest"] is not JsonObject request
                || request["RetCustModRq"] is not JsonObject rq
                || rq["RetCustModMainData"] is not JsonObject main
                || main["CustModData"] is not JsonObject customer
                || customer["CustId"] is not JsonValue cifValue
                || !cifValue.TryGetValue<string>(out var cif) || string.IsNullOrWhiteSpace(cif))
                return Failure("RetCustModKYC requires CustModData with a non-empty string CustId.");

            foreach (var field in new[] { "PhysicalState", "CustHealthCode", "Rating", "RiskRating" })
                if (customer[field] != null
                    && (customer[field] is not JsonValue value || !value.TryGetValue<string>(out _)))
                    return Failure($"{field} must be a string when supplied.");

            if (rq["RetailCustModRelatedData"] != null && rq["RetailCustModRelatedData"] is not JsonObject)
                return Failure("RetailCustModRelatedData must be an object when supplied.");
            var related = rq["RetailCustModRelatedData"] as JsonObject;
            foreach (var section in new[] { "DemographicModData", "CoreInterfaceInfo" })
                if (related?[section] != null && related[section] is not JsonObject)
                    return Failure($"{section} must be an object when supplied.");

            // Null properties emitted by the Admin DTO do not clear previously stored values.
            static bool HasValue(JsonNode? node) => node switch
            {
                null => false,
                JsonObject obj => obj.Any(p => HasValue(p.Value)),
                JsonArray arr => arr.Any(HasValue),
                JsonValue val when val.TryGetValue<string>(out var text) => !string.IsNullOrWhiteSpace(text),
                _ => true
            };
            if (!customer.Any(p => p.Key != "CustId" && p.Key != "AutoApproval" && HasValue(p.Value))
                && !HasValue(related))
                return Failure("No customer modification fields were supplied.");

            // Prepare the response before committing state; a missing/broken fixture cannot report success.
            var response = JsonNode.Parse(System.IO.File.ReadAllText("RetCustModKYC.json"))!.AsObject();
            var result = response["Data"]?["RetCustModResponse"]?["RetCustModRs"] as JsonObject;
            if (response["Code"]?.GetValue<string>() != "0" || result == null
                || result["Status"]?.GetValue<string>() != "SUCCESS")
                return Failure("RetCustModKYC response template is invalid.");
            cif = cif.Trim();
            result["CustId"] = cif;
            result["Desc"] = $"Retail Customer successfully updated with CIFID {cif}";
            var responseData = JsonSerializer.Deserialize<object>(response.ToJsonString())!;
            customer["CustId"] = cif;

            static JsonObject Merge(JsonObject previous, JsonObject update)
            {
                var merged = previous.DeepClone().AsObject();
                foreach (var field in update)
                {
                    if (field.Value == null) continue;
                    merged[field.Key] = field.Value is JsonObject incoming
                        ? Merge(merged[field.Key] as JsonObject ?? new JsonObject(), incoming)
                        : field.Value.DeepClone();
                }
                return merged;
            }
            RetCustModState.AddOrUpdate(cif, _ => Merge(new JsonObject(), rq),
                (_, previous) => Merge(previous, rq));
            return responseData;
        }
        catch (Exception)
        {
            return Failure("RetCustModKYC request could not be validated or stored.");
        }
    }

    private static string ApplyRetCustModToInquiry(string inquiryJson, string? base64Data)
    {
        try
        {
            if (RetCustModState.IsEmpty || string.IsNullOrWhiteSpace(base64Data)) return inquiryJson;
            var reqRoot = JsonNode.Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Data)));
            var custId = reqRoot?["RetCustInqRequest"]?["RetCustInqRq"]?["CustId"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(custId) || !RetCustModState.TryGetValue(custId.Trim(), out var mod)) return inquiryJson;

            var response = JsonNode.Parse(inquiryJson);
            var rs = response?["Data"]?["RetCustInqResponse"]?["RetCustInqRs"];
            var custDtls = rs?["RetCustDtls"]?.AsObject();
            var demoDtls = rs?["DemographicDtls"]?.AsObject();
            var main = mod["RetCustModMainData"]?["CustModData"];
            var demo = mod["RetailCustModRelatedData"]?["DemographicModData"];

            static void Copy(JsonObject? target, string targetName, JsonNode? source, string sourceName)
            {
                var value = source?[sourceName];
                if (target == null || value == null || !target.ContainsKey(targetName)) return;
                target[targetName] = JsonNode.Parse(value.ToJsonString());
            }

            // Reflect exact-name scalar fields only where the existing inquiry contract exposes them.
            static void CopyExposed(JsonObject? target, JsonNode? source)
            {
                if (target == null || source is not JsonObject fields) return;
                foreach (var field in fields)
                    if (field.Value is JsonValue) Copy(target, field.Key, fields, field.Key);
            }
            if (response?["Code"]?.GetValue<string>() != "0") return inquiryJson;
            CopyExposed(custDtls, main);
            CopyExposed(demoDtls, demo);
            CopyExposed(rs?["CoreInterfaceInfo"] as JsonObject, mod["RetailCustModRelatedData"]?["CoreInterfaceInfo"]);
            if (custDtls != null) custDtls["CustId"] = custId.Trim();
            Copy(custDtls, "ConstitutionCode", main, "ConstitutionCode");
            Copy(custDtls, "ConstitutionRefCode", main, "ConstitutionCode");
            Copy(custDtls, "Occupation", main, "Occupation");
            Copy(custDtls, "PriorityCode", main, "PriorityCode");
            Copy(custDtls, "Community", main, "Community");
            Copy(custDtls, "CommunityCode", main, "Community");
            Copy(custDtls, "PhysicalState", main, "PhysicalState");
            Copy(demoDtls, "NameOfEmployer", demo, "NameOfEmployer");
            Copy(demoDtls, "EmploymentStatus", demo, "EmploymentStatus");
            return response!.ToJsonString();
        }
        catch
        {
            return inquiryJson;
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RequestDto req)
    {
        if (!string.IsNullOrWhiteSpace(req.FunctionName) && ScenarioAwareFunctions.Contains(req.FunctionName))
        {
            var scenarioJson = TryGetScenarioResponse(req.FunctionName, req.Data);
            if (scenarioJson != null)
            {
                if (string.Equals(req.FunctionName, "RetailCIFInquiry", StringComparison.OrdinalIgnoreCase))
                    scenarioJson = ApplyRetCustModToInquiry(scenarioJson, req.Data);
                var scenarioData = JsonSerializer.Deserialize<object>(scenarioJson);
                return Ok(scenarioData);
            }
        }

        if (req.FunctionName == "AccountInquiryCAA")
        {
            var json = System.IO.File.ReadAllText("AccountInquiryCAA.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (req.FunctionName == "AccountInquiryODA")
        {
            var json = System.IO.File.ReadAllText("AccountInquiryODA.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (req.FunctionName == "CorporateCifInquiry")
        {
            var json = System.IO.File.ReadAllText("CorporateCifInquiry.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (req.FunctionName == "RetailCIFInquiry")
        {
            var json = ApplyRetCustModToInquiry(System.IO.File.ReadAllText("RetailCIFInquiry.json"), req.Data);
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (req.FunctionName == "RetailAccountInquiry")
        {
            var json = System.IO.File.ReadAllText("RetailAccountInquiry.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        // Added for Individual Admin Guardian Details "Verify" classification fetch
        // (AccountFunctionService.GetKycDetailByCIFAsync, FunctionName "GetKycDetailByCif" —
        // case mirrors the exact string that method sends). No simulator endpoint existed for this
        // function before; Admin was calling a function name the simulator didn't dispatch on.
        else if (string.Compare(req.FunctionName, "GetKycDetailByCif", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("GetKycDetailByCIF.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "Branch", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("Branch.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "Country", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("Country.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "GetCifByAccountNumber", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("GetCifByAccountNumber.json");
            //var json = System.IO.File.ReadAllText("GetCifByAccountNumber.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "IdIssueOrg", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("IdIssueOrg.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "CurrentAccountOpening", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("CurrentAccountOpening.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "CorporateCIFOpening", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("CorporateCIFOpening.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "GetConstitutionCodeByAccountNumber", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("GetConstitutionCodeByAccountNumber.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "AcctUnFreezeAdd", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("AcctUnFreezeAdd.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "RetCustModKYC", StringComparison.OrdinalIgnoreCase) == 0)
        {
            return Ok(StoreRetCustMod(req));
        }
        else if (string.Compare(req.FunctionName, "CorporateCustModKYC", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("CorporateCustModKYC.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "RetailAccountOpening", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("RetailAccountOpening.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "RetailCifOpening", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var retailCifOpeningError = ValidateRetailCifOpening(req.Data);
            if (retailCifOpeningError != null)
                return Ok(new { Code = "1", Message = retailCifOpeningError, Data = (object?)null });

            var json = System.IO.File.ReadAllText("RetailCifOpening.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "GetLinkedCifDetailByAccountNumber", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("GetLinkedCifDetailByAccountNumber.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "GetKycStatusByCIF", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("GetKycStatusByCIF.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "FamilyDetailsAdd", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("FamilyDetailsAdd.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "AcctFreezeAdd", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("AcctFreezeAdd.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "AccountFreezeInquiry", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("AccountFreezeInquiry.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "SignatureAdd", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("SignatureAddResponse.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (string.Compare(req.FunctionName, "GetKycDetailByCif", StringComparison.OrdinalIgnoreCase) == 0)
        {
            var json = System.IO.File.ReadAllText("GetKycDetailByCifResponse.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else if (req.FunctionName == "GetNIDToken")
        {
            var json = System.IO.File.ReadAllText("GetNIDToken.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
        }
        else
        {
            return NotFound($"Function '{req.FunctionName}' not found.");
        }
    }
}
