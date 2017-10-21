using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using netCoreWepApiAuthJWT.Model.Entities;
using netCoreWepApiAuthJWT.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace netCoreWepApiAuthJWT.Controllers
{
   

    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly myAppplicationContext _appDbContext;
        private readonly UserManager<AppUser> _userManager; /* UserManager is defined by default in Microsoft.AspNetCore.Identity;The UserManager<TUser> 
                                                            is a concrete class that manages the user. It is defined in the Microsoft.AspNet.Identity 
                                                            namespace.  This Class Creates, Updates, and Deletes the Users. 
                                                            It has methods to find a user by ID, User Name and email. It also provides the functionality
                                                            for adding  Claims, removing claims, add and removing roles, etc. It also generates password hash, Validates Users etc.*/

        public AccountsController(UserManager<AppUser> userManager, myAppplicationContext appDbContext)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
        }



        // POST api/accounts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AppUser userAppData = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.FirstName
            };

           // Call UserManager to create user
           var result = await _userManager.CreateAsync(userAppData, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(result);

            // Call Context on our Jobseeker DBset to add a JobSeeker
            await _appDbContext.JobSeekers.AddAsync(new JobSeeker { IdentityId = userAppData.Id, Location = model.Location });
            // Call Context on our Jobseeker DBset to SaveChanges.
            await _appDbContext.SaveChangesAsync();

            return new OkResult();
        }


    }
}
