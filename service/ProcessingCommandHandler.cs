using System;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using service.api;
using Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace service
{
    public class ProcessingCommandHandler : IMessageHandler<IProcessingCommand>
    {
        readonly ILogger<ProcessingCommandHandler> _log;
        readonly IMessageSender<ProcessingResponseBus> _responseSender;

        public ProcessingCommandHandler(
            ILogger<ProcessingCommandHandler> log,
            IMessageSender<ProcessingResponseBus> responseSender
        )
        {
            _log = log;
            _responseSender = responseSender;
        }

        public async Task HandleAsync(IMessage<IProcessingCommand> message, ExtendedMessageReceivedInfo info)
        {
            ProcessErrorCode errorCode = await RunProcessing(message.Body);
            var response = new ProcessingResponse
            {
                Command = message.Body,
                ErrorCode = errorCode
            };
            await _responseSender.SendAsync(response);
        }

        async Task<ProcessErrorCode> RunProcessing(IProcessingCommand command)
        {
            try
            {
                return ProcessErrorCode.Ok;
            }
            catch (Exception e)
            {
                return ProcessErrorCode.UnknownError;
            }
        }
    }
}
