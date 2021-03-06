﻿/*
    Copyright © 2010 The Divinity Project; 2013-2016 Dignity Team; 2016-2018 CentrinoGames.
    All rights reserved.
    https://github.com/CentrinoGames/Minerva
    http://www.ragezone.com


    This file is part of Minerva.

    Minerva is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    Minerva is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Minerva.  If not, see <http://www.gnu.org/licenses/>.
*/

#region Includes

using System;

#endregion

namespace Minerva
{
    partial class PacketProtocol
    {
        public static void SubPasswordFindRequest(PacketReader packet, PacketBuilder builder, ClientHandler client, EventHandler events)
        {
            var syncServer = client.Metadata["syncServer"] as SyncReceiver;

            var questionID = SubpassManagement.GetSubPassQuestion(syncServer, client.AccountID);

            builder.New(0x40C);
            {
                builder += questionID;
                builder += questionID;
                builder += 1;
            }

            client.Send(builder, "SubPasswordFindRequest");
        }
    }
}