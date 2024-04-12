using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Person.WebApi.Api.ValidationSpikeThree
{
    public class Person
    {
        [Required(ErrorMessage = "ID is required.")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "ID must be numeric.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; }

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int Age { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
        public string Email { get; set; }
    }

    public class NewPersonInputModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters long.")]
        public string Name { get; set; }

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120 years.")]
        public int Age { get; set; }

        [EmailAddress(ErrorMessage = "A valid email is required.")]
        public string Email { get; set; }
    }

    public class UpdatePersonInputModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters long.")]
        public string Name { get; set; }

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120 years.")]
        public int Age { get; set; }

        [EmailAddress(ErrorMessage = "A valid email is required.")]
        public string Email { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class PersonsController : ControllerBase
    {
        private static Dictionary<string, Person> _persons = new();

        public PersonsController()
        {
            _persons.Add("1", new Person { Id = "1", Name = "John Doe", Age = 30, Email = "john.doe@example.com" });
        }

        [HttpPost]
        public IActionResult CreatePerson([FromBody] NewPersonInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = (_persons.Count + 1).ToString();
            var person = new Person
            {
                Id = newId,
                Name = model.Name,
                Age = model.Age,
                Email = model.Email
            };
            _persons.Add(newId, person);
            return CreatedAtAction(nameof(GetPerson), new { id = newId }, person);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePerson(string id, [FromBody] UpdatePersonInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_persons.ContainsKey(id))
            {
                var person = _persons[id];
                person.Name = model.Name;
                person.Age = model.Age;
                person.Email = model.Email;
                return Ok(person);
            }
            return NotFound();
        }

        [HttpGet("{id}")]
        public IActionResult GetPerson(string id)
        {
            if (_persons.TryGetValue(id, out var person))
            {
                return Ok(person);
            }
            return NotFound();
        }
    }
}
