using Microsoft.AspNetCore.SignalR;
using TickTackToe.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TickTackToe.Hubs
{
    public class TickTackToeHub : Hub
    {
        private static ConcurrentDictionary<string, Game> Games = new ConcurrentDictionary<string, Game>();
        private static List<Users> OnlineUsers = new List<Users>();

        public async Task Send(string name)
        {
            var user = OnlineUsers.First(x => x.Id == Context.ConnectionId);
            user.Name = name;
            await Clients.All.SendAsync("onlinePlayers", JsonSerializer.Serialize(OnlineUsers.Where(x=> x.Name != null)));

            await Task.CompletedTask;
        }
        public async Task Challenge(string connectionId)
        {
            var user = OnlineUsers.First(x => x.Id == connectionId);
            var challengingPlayer = OnlineUsers.First(x => x.Id == Context.ConnectionId);
            await Clients.Client(user.Id).SendAsync("recieveChallenge", challengingPlayer.Id, challengingPlayer.Name);
        }

        public async Task Accept(string connectionId)
        {
            var user = OnlineUsers.First(x => x.Id == connectionId);
            var otherUser = OnlineUsers.First(x => x.Id == Context.ConnectionId);
            user.Status = "Playing";
            otherUser.Status = "Playing";
            var gameId = Guid.NewGuid().ToString();

            // Add users to group
            await Groups.AddToGroupAsync(connectionId, gameId);
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            var game = new Game { Id = gameId, Player1 = user, Player2 = otherUser };
            Games[gameId] = game;

            await Clients.Clients(connectionId, Context.ConnectionId).SendAsync("startGame", gameId);
            await Clients.All.SendAsync("onlinePlayers", JsonSerializer.Serialize(OnlineUsers.Where(x => x.Name != null)));
        }

        public override Task OnConnectedAsync()
        {
            OnlineUsers.Add(new Users() { Id = Context.ConnectionId });
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            OnlineUsers.Remove(OnlineUsers.First(x=> x.Id == Context.ConnectionId));
            Clients.All.SendAsync("onlinePlayers", JsonSerializer.Serialize(OnlineUsers.Where(x => x.Name != null)));
            return base.OnDisconnectedAsync(exception);
        }

        public async Task LeaveGame(string gameId)
        {
            if (Games.TryGetValue(gameId, out var game))
            {
                var leavingPlayer = OnlineUsers.First(x => x.Id == Context.ConnectionId);
                var remainingPlayer = game.Player1.Id == Context.ConnectionId ? game.Player2 : game.Player1;

                leavingPlayer.Status = "Free";
                remainingPlayer.Status = "Free";

                // Remove players from the group
                await Groups.RemoveFromGroupAsync(game.Player1.Id, gameId);
                await Groups.RemoveFromGroupAsync(game.Player2.Id, gameId);

                Games.TryRemove(gameId, out _);

                await Clients.Client(remainingPlayer.Id).SendAsync("gameEnded", "Opponent left the game");
                await Clients.All.SendAsync("onlinePlayers", JsonSerializer.Serialize(OnlineUsers.Where(x => x.Name != null)));
            }
        }

        public async Task MakeMove(string gameId, int row, int col, string playername)
        {
            string clubX = "";
            string clubY = "";
            if (col == 0)
            {
                clubY = "Barcelona";
            }
            else if (col == 1)
            {
                clubY = "Liverpool";
            }
            else if (col == 2)
            {
                clubY = "Man Utd";
            }

            if (row == 0)
            {
                clubX = "Real Madrid";
            }
            else if(row == 1)
            {
                clubX = "Bayern";
            }
            else if (row == 2)
            {
                clubX = "Dortmund";
            }
            Games.TryGetValue(gameId, out var game);
            if(game == null) { return; }
            var currentPlayer = game.CurrentTurn == game.Player1 ? game.Player1 : game.Player2;
            if (StorageHelper.IsPlayerInTheClub(clubX, clubY, playername))
            {
                
                if(currentPlayer.Id != Context.ConnectionId) 
                {
                    return;
                }
                if (!OnlineUsers.Any(x=> x.Id == currentPlayer.Id)) return;

                if (game.Board[row, col] == null)
                {
                    game.Board[row, col] = currentPlayer == game.Player1 ? "X" : "O";
                    game.CurrentTurn = currentPlayer == game.Player1 ? game.Player2 : game.Player1;
                    await Clients.Group(gameId).SendAsync("updateBoard", row, col, game.Board[row,col]);
                    var winner = CheckWinner(game.Board);
                    if (winner != null)
                    {
                        await Clients.Group(gameId).SendAsync("gameEnded", game.CurrentTurn.Name);
                        game.Player1.Status = "Free";
                        game.Player2.Status = "Free";
                        Games.TryRemove(gameId, out _);
                        await Clients.All.SendAsync("onlinePlayers", JsonSerializer.Serialize(OnlineUsers.Where(x => x.Name != null)));
                    }
                    else if (IsBoardFull(game.Board))
                    {
                        await Clients.Group(gameId).SendAsync("gameEnded", "Draw");
                        game.Player1.Status = "Free";
                        game.Player2.Status = "Free";
                        Games.TryRemove(gameId, out _);
                        await Clients.All.SendAsync("onlinePlayers", JsonSerializer.Serialize(OnlineUsers.Where(x => x.Name != null)));
                    }
                }
            }
            game.CurrentTurn = currentPlayer == game.Player1 ? game.Player2 : game.Player1;
        }

        private string CheckWinner(string[,] board)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] != null && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                    return board[i, 0];
                if (board[0, i] != null && board[0, i] == board[1, i] && board[1, i] == board[2, i])
                    return board[0, i];
            }
            if (board[0, 0] != null && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
                return board[0, 0];
            if (board[0, 2] != null && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
                return board[0, 2];

            return null;
        }

        private bool IsBoardFull(string[,] board)
        {
            foreach (var cell in board)
            {
                if (cell == null) return false;
            }
            return true;
        }

    }

    public class Users
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Status { get; set; } = "Free";
    }
    public class Game
    {
        public string Id { get; set; }
        public Users Player1 { get; set; }
        public Users Player2 { get; set; }
        public string[,] Board { get; set; } = new string[3, 3];
        public Users CurrentTurn { get; set; }
    }
}
