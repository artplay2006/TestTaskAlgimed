using System;
using System.Collections.Generic;

namespace TestTaskAlgimed.Models;

public partial class Step
{
    public int ID { get; set; }

    public int ModeId { get; set; }

    public int Timer { get; set; }

    public string? Destionation { get; set; }

    public int Speed { get; set; }

    public string Type { get; set; } = null!;

    public int Volume { get; set; }

    public virtual Mode Model { get; set; } = null!;
}
