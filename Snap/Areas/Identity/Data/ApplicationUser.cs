using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Snap.Areas.Identity.Data
{
    // Custom application user extending IdentityUser
    public class ApplicationUser : IdentityUser
    {
        // - UserName
        // - Email
        // - PhoneNumber
        // - PasswordHash
        // - and more
    }
}