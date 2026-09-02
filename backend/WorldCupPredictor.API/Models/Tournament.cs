using System;
using System.Collections.Generic;

namespace WorldCupPredictor.API.Models;

/// <summary>
/// Represents a World Cup tournament 
/// </summary>
public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Year { get; set; }
    public List<Group> Group { get; set; } = [];
    public List<Fixture> Fixtures { get; set; } = [];

}
