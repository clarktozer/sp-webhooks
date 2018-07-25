using System;
using System.ComponentModel.DataAnnotations;

namespace WebHooks.Data.Models
{
    public class ChangeToken
    {
        public Guid Id { get; set; }

        [Required]
        public string ListId { get; set; }

        [Required]
        public string WebId { get; set; }

        [Required]
        public string LastChangeToken { get; set; }
    }
}
