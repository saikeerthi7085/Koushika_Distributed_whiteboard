// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;


namespace Microsoft.Azure.SignalR.Samples.Whiteboard
{
    public class DrawHub : Hub
    {
        private Diagram diagram;

        public DrawHub(Diagram diagram)
        {
            this.diagram = diagram;
        }
        public override Task OnConnectedAsync()
        {
            var t = Task.WhenAll(diagram.Shapes.AsEnumerable().Select(l => Clients.Client(Context.ConnectionId).SendAsync("ShapeUpdated", l.Key, l.Value)));

            return t.ContinueWith(_ => Clients.All.SendAsync("UpdatedUserList", diagram.UserEnter()));
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string value = diagram.UserNamesList.First(x => x.Key == Context.ConnectionId).Value;                                                                                                                                                                                                                                                                                         
            diagram.UserNamesList.Remove(new KeyValuePair<string, string>(Context.ConnectionId, value));

            IDictionary<string, string> dictionaryobjet = diagram.UserNamesList.ToDictionary(pair => pair.Key, pair => pair.Value);
            return Clients.Others.SendAsync("UpdatedUserList", dictionaryobjet);
        }

        public async Task PatchShape(string id, List<int> data)
        {
            diagram.Shapes[id].Data.AddRange(data);
            await Clients.Others.SendAsync("ShapePatched", id, data);
        }

        public async Task UpdateShape(string id, Diagram.Shape shape)
        {
            diagram.Shapes[id] = shape;
            await Clients.Others.SendAsync("ShapeUpdated", id, shape);
        }

        public async Task RemoveShape(string id)
        {
            diagram.Shapes.Remove(id, out _);
            await Clients.Others.SendAsync("ShapeRemoved", id);
        }

        public async Task Clear()
        {
            diagram.Shapes.Clear();

            await Clients.All.SendAsync("Clear");
        }

        public async Task SendMessage(string name, string message)
        {
            await Clients.Others.SendAsync("NewMessage", name, message);
        }

        public async Task AddUSerDetails(string name)
        {
            diagram.UserNamesList.Add(new KeyValuePair<string, string>(Context.ConnectionId, name));
            IDictionary<string, string> dictionaryobjet = diagram.UserNamesList.ToDictionary(pair => pair.Key, pair => pair.Value);
            await Clients.All.SendAsync("UpdatedUserList", dictionaryobjet);

        }

        public async Task CreateNew()
        {
            await Clients.Others.SendAsync("CreateNewBoard", diagram.UserNamesList);

        }
        public async Task Savedata()
        {
            string Name = "Image" + DateTime.Now;

            diagram.SavedDate.Add(Name, diagram.Shapes.Select(x => x.Value).FirstOrDefault());
            diagram.Shapes.Clear();
            await Clients.Client(diagram.UserNamesList.Select(x => x.Key).First()).SendAsync("SavedwhiteboardData", diagram.SavedDate);

        }
        public async Task EnableLeaderrights(string leaderconnectionid)
        {

            await Clients.Client(leaderconnectionid).SendAsync("CheckLeaderRights");
        }
    }
}
