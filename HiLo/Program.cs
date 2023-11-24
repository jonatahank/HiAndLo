// See https://aka.ms/new-console-template for more information
var menu = new Menu();
menu.GameSetupAndStart();





/// <summary>
/// It will create the menu of the game and add the players
/// </summary>
public class Menu
{
	public int Quantity { get; set; }
	public List<Player> Players { get; set; }
	public IGame Game { get; set; }

	void StartGame()
	{
		if (Quantity == 1)
			Game = new SinglePlayerGame(Players[0]);
		else
			Game = new MultiplayerGame(Players);
		Game.ExecuteGame();
	}

	void AddPlayers()
	{
		Console.WriteLine("How Many players will play?");
		Console.WriteLine();
		var answerQuantity = Console.ReadLine();
		var wasParsed = int.TryParse(answerQuantity, out int quantity);
		if (!wasParsed)
		{
			Console.WriteLine("Invalid value, the value should be a integer!");
			Console.WriteLine();
			AddPlayers();
		}

		Quantity = quantity;

		for (int i = 1; i <= quantity; i++)
		{
			AddName(i);
		}

	}

	public void AddName(int number)
	{
		Console.WriteLine($"Set a name for player{number}");
		Console.WriteLine();
		string name = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(name))
		{
			Console.WriteLine("Invalid name, set at least one character name");
			Console.WriteLine();
			AddPlayers();
		}
		if (Players.Count > 0 && Players.Any(c => c.Name.Equals(name)))
		{
			Console.WriteLine("this name is already in the list");
			Console.WriteLine();
			AddPlayers();
		}
		Players.Add(new Player(name));
	}

	public void GameSetupAndStart()
	{
		Players = new List<Player>();
		AddPlayers();
		StartGame();
		if (ShouldStartANewGame())
		{
			GameSetupAndStart();
		}
	}

	private bool ShouldStartANewGame()
	{
		Console.WriteLine("Would you like to start a new Game? Y/N");
		var answer = Console.ReadLine();

		if (answer.ToLower().Equals("y"))
		{
			return true;
		}
		return false;
	}

}

/// <summary>
/// Create a game type single player
/// </summary>
public class SinglePlayerGame : Base, IGame
{
	private Player Player { get; set; }
	public SinglePlayerGame(Player player)
	{
		this.Player = player;

	}
	public void ExecuteGame()
	{
		var value = GetChoice(Player.Name);
		var result = GameBaseBehaviourAndReturnWonOrContinues(value, Player);
		if (result == GameStatus.Won)
		{
			if (Player.TotalMatches > 1)
				Player.ShowPlayerStatitics();
			else { Console.WriteLine("You won with a perfect match!"); Console.WriteLine(); }
			return;
		}
		else
			ExecuteGame();

	}
}
/// <summary>
/// It will create the multiplayer version
/// </summary>
public class MultiplayerGame : Base, IGame
{
	private List<Player> Players { get; set; }
	public MultiplayerGame(List<Player> players)
	{
		this.Players = players;
	}
	public void ExecuteGame()
	{
		for (int i = 0; i < Players.Count; i++)
		{
			var value = GetChoice(Players[i].Name);
			var result = GameBaseBehaviourAndReturnWonOrContinues(value, Players[i]);
			if (result == GameStatus.Won)
			{
				if (Players[i].TotalMatches > 1)
					Players.ForEach(p => { p.ShowPlayerStatitics(); });
				else
				{
					Console.WriteLine("##################################################");
					Console.WriteLine($"Player {Players[i].Name} won with a perfect match!");
					Console.WriteLine();
					foreach (var item in Players.Where(w => w.Id != Players[i].Id))
					{
						item.ShowPlayerStatitics();
					}
				}

				return;
			}
		}
		ExecuteGame();
	}
}

public interface IGame
{
	void ExecuteGame();
}
/// <summary>
/// Base of the game with the shared logic
/// </summary>
public class Base
{
	readonly Random rand = new();
	readonly int[] minMax = { 0, 1000 };
	private Dictionary<Guid, int> MisteriousNumber = new Dictionary<Guid, int>();
	protected int GetChoice(string name)
	{
		Console.WriteLine($"player: {name}, Guess a number between 0 and 1000");
		Console.WriteLine();
		var value = Console.ReadLine();
		var wasParsed = int.TryParse(value, out var choice);

		if (!wasParsed || choice > 1000 || choice < 0)
		{
			Console.WriteLine("Invalid value, the value should be a integer between 0 and 1000");
			Console.WriteLine();
			GetChoice(name);
		}

		return choice;
	}
	protected GameStatus GameBaseBehaviourAndReturnWonOrContinues(int value, Player player)
	{
		var mysteriousNumber = GetMysteriousNumberOrSet(player.Id);
		if (value != mysteriousNumber)
		{
			if (value > mysteriousNumber)
				player.IncreaseHiAndTotal();
			else player.IncreaseLoAndTotal();

			return GameStatus.Continues;
		}
		player.Status = GameStatus.Won;
		return GameStatus.Won;

	}
	protected int GetMysteriousNumberOrSet(Guid id)
	{
		if (!MisteriousNumber.ContainsKey(id))
		{
			MisteriousNumber.Add(id, rand.Next(minMax[0], minMax[1]));
		}
		return MisteriousNumber[id];
	}
}

public enum GameStatus
{
	Won,
	Lost,
	Continues
}

/// <summary>
/// Play info and statistics
/// </summary>

public class Player
{

	public string Name { get; set; }
	public int TotalMatches { get; set; }
	private int TotalLo;
	private int TotalHi;
	public GameStatus Status { get; set; }
	public readonly Guid Id;

	private double PercentageLo
	{
		get
		{
			return TotalLo != 0 ? ((TotalLo * 100) / TotalMatches) : 0;
		}
	}
	private double PercentageHi
	{
		get
		{
			return TotalHi != 0 ? ((TotalHi * 100) / TotalMatches) : 0;
		}
	}
	public Player(string name)
	{
		Name = name;
		Id = Guid.NewGuid();
		Status = GameStatus.Lost;
	}
	public void IncreaseHiAndTotal()
	{
		Console.WriteLine($"the choice of {Name} is Hi");
		Console.WriteLine();
		TotalMatches++;
		TotalHi++;
	}
	public void IncreaseLoAndTotal()
	{
		Console.WriteLine($"the choice of {Name} is Lo");
		Console.WriteLine();
		TotalMatches++;
		TotalLo++;
	}
	public void ShowPlayerStatitics()
	{
		Console.WriteLine();
		Console.WriteLine($"The player {Name} {Status} ");
		Console.WriteLine($"The player {Name}, betted {PercentageHi}% Hi and {PercentageLo}% Lo ");
		Console.WriteLine();
	}
}