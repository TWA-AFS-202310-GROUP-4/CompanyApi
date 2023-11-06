﻿using Microsoft.AspNetCore.Mvc;

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
        public ActionResult<List<Company>> GetAllCompanies()
        {
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetCompanyById(string id)
        {
            var index=companies.FindIndex(company => company.Id == id);
            if(index == -1)
            {
                return NoContent();
            }
            return Ok(companies[index]);
        }
    }
}
