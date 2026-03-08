using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ConnectController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RequestDto req)
    {
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
        else
        {
            return NotFound($"Function '{req.FunctionName}' not found.");
        }
    }
}
