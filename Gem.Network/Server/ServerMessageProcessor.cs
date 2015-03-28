﻿﻿using System;
using Lidgren.Network;
using Gem.Network.Messages;
using System.Net;
using System.Collections.Generic;
using Gem.Network.Utilities;
using Gem.Network.Containers;
using Gem.Network.Utilities.Loggers;
using Gem.Network.Extensions;
using Gem.Network.Events;

namespace Gem.Network.Server
{
    public class ServerMessageProcessor : IMessageProcessor
    {

        #region Declarations

        private readonly IServer server;

        private readonly IAppender Write;
       
        #endregion


        #region Constructor

        public ServerMessageProcessor(IServer server)
        {
            this.server = server;

            Write = new ActionAppender(GemNetworkDebugger.Echo);
        }

        #endregion


        #region Messages

        public void ProcessNetworkMessages()
        {
            NetIncomingMessage im;
            server.Wait();

            while ((im = this.server.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        var approvalMsg = new ConnectionApprovalMessage(im);
                        Write.Info("Incoming Connection");
                        GemServer.ServerConfiguration[GemNetwork.ActiveProfile].OnIncomingConnection(server, im.SenderConnection, approvalMsg);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        switch ((NetConnectionStatus)im.ReadByte())
                        {
                            case NetConnectionStatus.Connected:
                                //im.SenderConnection.Approve();
                                Write.Info("{0} Connected", im.SenderConnection);
                                break;
                            case NetConnectionStatus.Disconnected:
                                Write.Info(im.SenderConnection + " status changed. " + (NetConnectionStatus)im.SenderConnection.Status);
                                GemServer.ServerConfiguration[GemNetwork.ActiveProfile].OnClientDisconnect(server, im.SenderConnection,im.ReadString());
                                break;
                            case NetConnectionStatus.RespondedConnect:
                                Write.Info(im.SenderConnection + " status changed. " + (NetConnectionStatus)im.SenderConnection.Status);
                                break;
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        byte id = im.ReadByte();

                        if (id == GemNetwork.NotificationByte)
                        {
                            GemServer.ServerConfiguration[GemNetwork.ActiveProfile].HandleNotifications(server, im.SenderConnection, new Notification(im));
                        }
                        else if (GemServer.MessageFlow[GemNetwork.ActiveProfile, MessageType.Data].HasKey(id))
                        {
                            GemServer.MessageFlow[GemNetwork.ActiveProfile, MessageType.Data, id]
                                     .HandleIncomingMessage(im);
                        }
                        else
                        {
                            var msg = server.CreateMessage();
                            msg.Write(im);
                            server.SendAndExclude(msg, im.SenderConnection);
                        }
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                        Write.Warn(im.ReadString());
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        Write.Error(im.ReadString());
                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        //notify the client 
                        break;
                }
                this.server.Recycle(im);
            }
        }

        #endregion
        
    }
        internal static class MessageArgumentsExtensions
        {
            /// <summary>
            /// Decodes and handles incoming messages
            /// </summary>
            /// <param name="args">The message arguments</param>
            /// <param name="message">The net incoming message to decode and handle</param>
            internal static void HandleIncomingMessage(this MessageArguments args, NetIncomingMessage message)
            {
                var readableMessage = MessageSerializer.Decode(message, args.MessagePoco);
                args.MessageHandler.Handle(readableMessage);//.ReadAllProperties());
            }
        }

}