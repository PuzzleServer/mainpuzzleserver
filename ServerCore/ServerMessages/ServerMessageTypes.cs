using System;

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

/// <summary>
/// Message requesting all presence state
/// </summary>
public class GetPresenceState
{
    /// <summary>
    /// The client requesting the presence state
    /// </summary>
    public string Client { get; set; }
}

/// <summary>
/// Full presence information for all clients
/// </summary>
public class AllPresenceState
{
    /// <summary>
    /// The presence information for all clients
    /// </summary>
    public PresenceMessage[] AllPresence { get; set; }
}

/// <summary>
/// Content of a notification, intended to be broadcast to all players in a specific event, all players on a specific team, or a specific player.
/// </summary>
public class Notification
{
    public string ID { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The event the notification is for.
    /// </summary>
    public int? EventID { get; set; }

    /// <summary>
    /// The team the notification is for.
    /// </summary>
    public int? TeamID { get; set; }

    /// <summary>
    /// The player the notification is for.
    /// </summary>
    public int? PlayerID { get; set; }

    /// <summary>
    /// The title of the notification.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The content of the notification.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The link that should be followed if the notification is clicked.
    /// </summary>
    public string LinkUrl { get; set; }
}
