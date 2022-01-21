namespace PokeCards.Data;

public class PokemonTypes
{
    public static PokemonType Bug => new("Bug", "Images/Logos/Icon_Bug.svg.png");
    public static PokemonType Dark = new("Dark", "Images/Logos/Icon_Dark.svg.png");
    public static PokemonType Dragon = new("Dragon", "Images/Logos/Icon_Dragon.svg.png");
    public static PokemonType Electric = new("Electric", "Images/Logos/Icon_Electric.svg.png");
    public static PokemonType Fairy = new("Fairy", "Images/Logos/Icon_Fairy.svg.png");
    public static PokemonType Fighting = new("Fighting", "Images/Logos/Icon_Fighting.svg.png");
    public static PokemonType Fire = new("Fire", "Images/Logos/Icon_Fire.svg.png");
    public static PokemonType Flying = new("Flying", "Images/Logos/Icon_Flying.svg.png");
    public static PokemonType Ghost = new("Ghost", "Images/Logos/Icon_Ghost.svg.png");
    public static PokemonType Grass = new("Grass", "Images/Logos/Icon_Grass.svg.png");
    public static PokemonType Ground = new("Ground", "Images/Logos/Icon_Ground.svg.png");
    public static PokemonType Ice = new("Ice", "Images/Logos/Icon_Ice.svg.png");
    public static PokemonType Normal = new("Normal", "Images/Logos/Icon_Normal.svg.png");
    public static PokemonType Poison = new("Poison", "Images/Logos/Icon_Poison.svg.png");
    public static PokemonType Psychic = new("Psychic", "Images/Logos/Icon_Psychic.svg.png");
    public static PokemonType Rock = new("Rock", "Images/Logos/Icon_Rock.svg.png");
    public static PokemonType Steel = new("Steel", "Images/Logos/Icon_Steel.svg.png");
    public static PokemonType Water = new("Water", "Images/Logos/Icon_Water.svg.png");

    public static List<PokemonType> GetAll() => new List<PokemonType>
    {
        Bug, Dark, Dragon, Electric, Fairy, Fighting, Fire, Flying, Ghost, 
        Grass, Ground, Ice, Normal, Poison, Psychic, Rock, Steel, Water
    };
}

public record PokemonType(string Name, string IconUrl);
