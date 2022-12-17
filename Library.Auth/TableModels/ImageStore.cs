using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Library.Database.Auth;

namespace Library.Database.Auth.TableModels
{
    public class ImageStore
    {
        [Key]
        public string ImageId { get; set; }
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }

        public string MimeType { get; set; }
        public DateTime UploadedOn { get; set; }

        // requires a public ICollection Reference to this class on SecureUser in order for ef to create the foreign key
        [ForeignKey("UploadedBy")]
        public SecureUser SecureUser { get; set; }

        public static string InsertAndProvideId(ApplicationDbContext context, ClaimsPrincipal secureUser, UserManager<Library.Database.Auth.SecureUser> userManager, IFormFile file)
        {
            if (IsImage(file) && secureUser.Identity != null && secureUser.IsInRole("God"))
            {
                string fileName;
                fileName = file.FileName;
                string fileExtension = Path.GetExtension(fileName);

                string imageId = $"{Guid.NewGuid()}";

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();

                    SecureUser su = userManager.GetUserAsync(secureUser).Result;

                    ImageStore imageStore = new ImageStore()
                    {
                        ImageId = imageId,
                        Bytes = fileBytes,
                        Extension = fileExtension,
                        Name = fileName,
                        SecureUser = su,
                        UploadedOn = DateTime.UtcNow,
                        MimeType = file.ContentType.ToLower(),
                    };
                    context.Add<ImageStore>(imageStore);
                    context.SaveChanges();
                }
                
                return imageId;
            }
            else
            {
                return string.Empty;
            }
        }

        public static ImageStore? GetImage(ApplicationDbContext context, string imageId)
        {
            ImageStore? image = context.ImageStore.FirstOrDefault(p => p.ImageId == imageId);
            return image;
        }

        private static bool IsImage(IFormFile file)
        {
            int ImageMinimumBytes = 512;
            //  Check the image mime types.  Add to as you see fit.. missing native iphone photos...
            if (file.ContentType.ToLower() != "image/jpg" &&
                        file.ContentType.ToLower() != "image/jpeg" &&
                        file.ContentType.ToLower() != "image/pjpeg" &&
                        file.ContentType.ToLower() != "image/gif" &&
                        file.ContentType.ToLower() != "image/x-png" &&
                        file.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //  Check the image extension
            if (Path.GetExtension(file.FileName).ToLower() != ".jpg"
                && Path.GetExtension(file.FileName).ToLower() != ".png"
                && Path.GetExtension(file.FileName).ToLower() != ".gif"
                && Path.GetExtension(file.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //  Attempt to read the file and check the first bytes
            try
            {
                if (!file.OpenReadStream().CanRead)
                {
                    return false;
                }
                //check whether the image size exceeding the limit or not
                if (file.Length < ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new byte[ImageMinimumBytes];
                file.OpenReadStream().Read(buffer, 0, ImageMinimumBytes);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using (var bitmap = new System.Drawing.Bitmap(file.OpenReadStream()))
                    {
                    }
                }                
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                file.OpenReadStream().Position = 0;
            }

            return true;
        }

    }    
}
