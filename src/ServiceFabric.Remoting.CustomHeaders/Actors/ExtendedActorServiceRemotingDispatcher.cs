﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.V2;
using Microsoft.ServiceFabric.Actors.Remoting.V2.Runtime;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Remoting.CustomHeaders.Actors
{
    /// <summary>
    /// <see cref="ActorServiceRemotingDispatcher"/> that operates on the receiving side
    /// </summary>
    public class ExtendedActorServiceRemotingDispatcher : ActorServiceRemotingDispatcher
    {
        public ExtendedActorServiceRemotingDispatcher(ActorService actorService, IServiceRemotingMessageBodyFactory serviceRemotingRequestMessageBodyFactory)
            : base(actorService, serviceRemotingRequestMessageBodyFactory)
        {
        }

        public override void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
        {
            RemotingContext.FromRemotingMessage(requestMessage);
            base.HandleOneWayMessage(requestMessage);
        }

        public override async Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext,
            IServiceRemotingRequestMessage requestMessage)
        {
            var header = (IActorRemotingMessageHeaders)requestMessage.GetHeader();
            var methodName = string.Empty;
            if (header.TryGetHeaderValue(CustomHeaders.MethodHeader, out byte[] headerValue))
            {
                methodName = Encoding.ASCII.GetString(headerValue);
            }

            RemotingContext.FromRemotingMessage(requestMessage);
            if (BeforeHandleRequestResponseAsync != null)
                await BeforeHandleRequestResponseAsync.Invoke(requestMessage, header.ActorId, methodName);
            var responseMessage = await base.HandleRequestResponseAsync(requestContext, requestMessage);
            if (AfterHandleRequestResponseAsync != null)
                await AfterHandleRequestResponseAsync.Invoke(responseMessage, header.ActorId, methodName);

            return responseMessage;
        }

        public Func<IServiceRemotingRequestMessage, ActorId, string, Task> BeforeHandleRequestResponseAsync { get; set; }

        public Func<IServiceRemotingResponseMessage, ActorId, string, Task> AfterHandleRequestResponseAsync { get; set; }
    }
}
