using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace CompanyApiTest;

public class CompanyApiTest
{
    private HttpClient httpClient;

    public CompanyApiTest()
    {
        WebApplicationFactory<Program> webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
    }

    [Fact]
    public async Task Should_return_created_company_with_status_201_when_create_cpmoany_given_a_company_name()
    {
        // Given
        await ClearDataAsync();
        Company companyGiven = new Company("BlueSky Digital Media");
        
        // When
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
            "/api/companies", 
            SerializeObjectToContent(companyGiven)
        );
       
        // Then
        Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
        Company? companyCreated = await DeserializeTo<Company>(httpResponseMessage);
        Assert.NotNull(companyCreated);
        Assert.NotNull(companyCreated.Id);
        Assert.Equal(companyGiven.Name, companyCreated.Name);
    }

    [Fact]
    public async Task Should_return_bad_reqeust_when_create_company_given_a_existed_company_name()
    {
        // Given
        await ClearDataAsync();
        Company companyGiven = new Company("BlueSky Digital Media");

        // When
        await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
            "/api/companies", 
            SerializeObjectToContent(companyGiven)
        );
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
    }

    [Fact]
    public async Task Should_return_bad_reqeust_when_create_company_given_a_company_with_unknown_field()
    {
        // Given
        await ClearDataAsync();
        StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
      
        // When
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("/api/companies", content);
       
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
    }

    [Fact]
    public async Task Should_return_company_name_when_GetCompanyById_given_a_company_id()
    {
        //Given
        await ClearDataAsync();
        Company companyGiven1 = new Company("BlueSky Digital Media");
        var httpMessage = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven1));
        var createdCompany = DeserializeTo<Company>(httpMessage).Result;
        string id = createdCompany.Id;

        //When
        HttpResponseMessage getCompanyByIdResponseMessage = await httpClient.GetAsync("/api/companies/" + id);
        var queriedCompany = DeserializeTo<Company>(getCompanyByIdResponseMessage).Result;

        //Then
        Assert.Equal(createdCompany.Name, queriedCompany.Name);
    }


    [Fact]
    public async Task Should_return_new_company_name_when_UpdateCompany_given_a_new_company_info()
    {
        //Given
        await ClearDataAsync();
        Company companyGiven1 = new Company("Google");
        var httpMessage = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven1));
        var createdCompany = DeserializeTo<Company>(httpMessage).Result;
        string id = createdCompany.Id;

        //When
        companyGiven1.Name = "SLB";
        companyGiven1.Id = id;
        var httpResponseMessage = await httpClient.PutAsJsonAsync<Company>("/api/companies", companyGiven1);

        //Then
        Assert.Equal(HttpStatusCode.NoContent, httpResponseMessage.StatusCode);
    }

    
    [Fact]
    public async Task Should_return_corrosponding_campanies_when_GetCompanyWithPagination_given_a_pageSize_and_pageNum()
    {
        //Given
        await ClearDataAsync();
        await httpClient.PostAsync("/api/companies", SerializeObjectToContent(new Company("BlueSky Digital Media")));
        await httpClient.PostAsync("/api/companies", SerializeObjectToContent(new Company("SLB")));
        await httpClient.PostAsync("/api/companies", SerializeObjectToContent(new Company("Google")));
        await httpClient.PostAsync("/api/companies", SerializeObjectToContent(new Company("Amazon")));
        //When
        HttpResponseMessage getCompaniesWithPaginationResponseMessage = await httpClient.GetAsync("/api/companies?pageSize=2&pageNumber=2");

        var queriedCompanies = DeserializeTo<List<Company>>(getCompaniesWithPaginationResponseMessage).Result;

        //Then
        Assert.Equal("Google", queriedCompanies[0].Name);
        Assert.Equal("Amazon", queriedCompanies[1].Name);
    }

    [Fact] 
    public async Task Should_add_new_employee_when_AddEmployeeToACompany_givne_a_new_employee()
    {
        //Given
        await ClearDataAsync();
        Company companyGiven1 = new Company("BlueSky Digital Media");
        var httpMessage = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven1));
        var createdCompany = DeserializeTo<Company>(httpMessage).Result;
        string id = createdCompany.Id;
        //When
        Employee employee = new Employee("Bob", "1");
        HttpResponseMessage responseMessage = await httpClient.PutAsJsonAsync("/api/companies/" + id, employee);

        //Then
        Assert.Equal(HttpStatusCode.NoContent, responseMessage.StatusCode);

    }

    [Fact]
    public async Task Should_delete_an_employee_when_DeleteEmployeeofACompany_givne_an_employee()
    {
        //Given
        await ClearDataAsync();
        Company companyGiven1 = new Company("BlueSky Digital Media");
        var httpMessage = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven1));
        var createdCompany = DeserializeTo<Company>(httpMessage).Result;
        string id = createdCompany.Id;

        Employee employee = new Employee("Bob", "1");
        await httpClient.PutAsJsonAsync("/api/companies/" + id, employee);
        //When
        HttpResponseMessage responseMessage = await httpClient.DeleteAsync($"/api/companies/{id}/employees/1");

        //Then
        Assert.Equal(HttpStatusCode.NoContent, responseMessage.StatusCode);
    }

    [Fact]
    public async Task Should_show_employeeList_when_GetEmployeeList_given_an_companyId()
    {
        //Given
        await ClearDataAsync();
        Company companyGiven1 = new Company("BlueSky Digital Media");
        var httpMessage = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven1));
        var createdCompany = DeserializeTo<Company>(httpMessage).Result;
        string id = createdCompany.Id;

        Employee employee = new Employee("Bob", "1");
        await httpClient.PutAsJsonAsync("/api/companies/" + id, employee);
        //When
        HttpResponseMessage responseMessage = await httpClient.GetAsync($"/api/companies/company/{id}");
        var queriedCompanies = DeserializeTo<List<Company>>(responseMessage).Result;
        //Then
        Assert.Equal("Bob", queriedCompanies[0].Name);
    }

    private async Task<T?> DeserializeTo<T>(HttpResponseMessage httpResponseMessage)
    {
        string response = await httpResponseMessage.Content.ReadAsStringAsync();
        T? deserializedObject = JsonConvert.DeserializeObject<T>(response);
        return deserializedObject;
    }

    private static StringContent SerializeObjectToContent<T>(T objectGiven)
    {
        return new StringContent(JsonConvert.SerializeObject(objectGiven), Encoding.UTF8, "application/json");
    }

    private async Task ClearDataAsync()
    {
        await httpClient.DeleteAsync("/api/companies");
    }
}