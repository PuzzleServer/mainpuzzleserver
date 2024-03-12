using System;

public class ExampleMessage
{
	public string SomeInfo { get; set; }
	public int TeamId { get; set; }
	public int PuzzleUserId { get; set; }
}

/// <summary>
/// User presence on a puzzle
/// </summary>
public enum PresenceType
{
    /// <summary>
    /// The puzzle is in the foreground
    /// </summary>
    Active,

    /// <summary>
    /// The puzzle has been closed or idle long enough that the browser slept the tab
    /// </summary>
    Disconnected,
}

/// <summary>
/// Updates on who is actively looking at a particular puzzle
/// </summary>
public class PresenceMessage
{
    /// <summary>
    /// Differentiates instances if the user is looking at the same puzzle in multiple tabs
    /// </summary>
    public Guid PageInstance { get; set; }

    /// <summary>
    /// User looking at the puzzle
    /// </summary>
    public int PuzzleUserId { get; set; }

    /// <summary>
    /// Team to sync this to
    /// </summary>
    public int TeamId { get; set; }

    /// <summary>
    /// Puzzle the user is present on
    /// </summary>
    public int PuzzleId { get; set; }

    /// <summary>
    /// The new presence type
    /// </summary>
    public PresenceType PresenceType { get; set; }
}
