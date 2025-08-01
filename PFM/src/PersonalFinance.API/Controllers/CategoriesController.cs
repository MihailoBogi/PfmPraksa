using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Application.Common;
using PersonalFinance.Application.Contracts;
using PersonalFinance.Application.Interfaces;
using PersonalFinance.Infrastructure.Services;
namespace PersonalFinance.API.Controllers
{
    [Route("categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryImporter _importer;
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryImporter importer, ICategoryService categoryService)
        {
            _importer = importer;
            _categoryService = categoryService;
        }

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BusinessErrorResponse), 440)]
    //    [ProducesResponseType(typeof(BusinessProblemResponse), 409)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BusinessException(
                    "file-missing",
                    "CSV fajl je obavezan",
                    "Neophodno je poslati fajl pod imenom 'file' u form-data body-u."
                );

            await _importer.ImportAsync(file);
            return Ok(new { message = "Categories imported" });
        }

        [HttpGet]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCategories([FromQuery(Name = "parent-code")] string? parentId)
        {
            try
            {
                var items = await _categoryService.GetByParentCodeAsync(parentId);
                return Ok(new CategoriesResponse { Items = items });
            }
            catch (ValidationException vex)
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Errors = vex.Errors
                });
            }
        }
    }
}
