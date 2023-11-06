﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

        [HttpPost]
        public ActionResult<Company> Create(CreateCompanyRequest request)
        {
            if (companies.Exists(company => company.Name.Equals(request.Name)))
            {
                return BadRequest();
            }
            Company companyCreated = new Company(request.Name);
            companies.Add(companyCreated);
            return StatusCode(StatusCodes.Status201Created, companyCreated);
        }

        [HttpDelete]
        public void ClearData()
        {
            companies.Clear();
        }

        [HttpGet]
        public ActionResult<List<Company>> GetAllCompanies([FromQuery] int? pageSize, [FromQuery] int? pageIndex)
        {
            if (pageSize != null && pageSize != null)
            {
                int pages = (int)Math.Ceiling((double)companies.Count / (double)pageSize);
                if (pageSize <= 0 || pageIndex < 1 || pages < pageIndex)
                {
                    return BadRequest();
                }

                int startIndex = (int)((pageIndex - 1) * pageSize);
                int companiesToTake = (int)((pageSize < (companies.Count - startIndex)) ? pageSize : companies.Count);
                List<Company> filterdCompanies = companies.Skip((int)((pages - 1) * pageSize)).Take(companiesToTake).ToList();
                return Ok(filterdCompanies);
            }
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetCompanyById(string id)
        {
            var index = companies.FindIndex(company => company.Id == id);
            if (index == -1)
            {
                return NoContent();
            }
            return Ok(companies[index]);
        }


    }
}
