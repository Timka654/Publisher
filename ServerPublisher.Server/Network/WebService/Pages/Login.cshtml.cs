using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace ServerPublisher.Server.Network.WebService.Pages
{
    public class LoginModel : PageModel
    {
        public LoginModel()
        {

        }

        [BindProperty, Required] public IFormFile signFile { get; set; }

        public void OnGet()
        {
        }

        public async ValueTask OnPostAsync()
        {
            if (!ModelState.IsValid)
                return;

            using (MemoryStream ms = new MemoryStream())
            {

                await signFile.CopyToAsync(ms);
            }
        }
    }
}
