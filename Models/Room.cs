namespace BattleshipBackend.Models;

public class Room
{
    public string Name { get; }
    public User Owner { get; }
    public User? Opponent { get; set; }

    public Room(string name, User owner)
    {
        Name = name;
        Owner = owner;
    }
}