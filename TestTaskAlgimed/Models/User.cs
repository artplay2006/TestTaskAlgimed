using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestTaskAlgimed.Models;

public partial class User
{
    [Required(AllowEmptyStrings = false)]
    public string Login { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = null!;

    public int Id { get; set; }
}
