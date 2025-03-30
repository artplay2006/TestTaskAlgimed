using System;
using System.Collections.Generic;

namespace TestTaskAlgimed.Models;

public partial class Mode
{
    public int ID { get; set; }

    public string Name { get; set; } = null!;

    public int MaxBottleNumber { get; set; }

    public int MaxUsedTips { get; set; }

    public virtual ICollection<Step> Steps { get; set; } = new List<Step>();
}
