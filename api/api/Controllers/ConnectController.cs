using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RequestDto req)
    {
        if (!string.IsNullOrWhiteSpace(req.FunctionName) && ScenarioAwareFunctions.Contains(req.FunctionName))
        {
            var scenarioJson = TryGetScenarioResponse(req.FunctionName, req.Data);
            if (scenarioJson != null)
            {
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
            var json = System.IO.File.ReadAllText("RetailCIFInquiry.json");
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
            var json = System.IO.File.ReadAllText("RetCustModKYC.json");
            var data = JsonSerializer.Deserialize<object>(json);
            return Ok(data);
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
