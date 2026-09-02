using System;

namespace WorldCupPredictor.API.Models;

/// <summary>
/// Represents a country in the World Cup.
/// </summary>
public class Country
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string IsoCode { get; set; } = "";
	public string flagPath { get; set; } = "";
	public int? OppCode { get; set; }
}
