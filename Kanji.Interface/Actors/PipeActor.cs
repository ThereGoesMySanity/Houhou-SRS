using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kanji.Common.Helpers;
using Kanji.Common.Models;
using Kanji.Interface.Helpers;
using Kanji.Interface.Models;
using Kanji.Interface.Utilities;
using Microsoft.Extensions.Logging;

namespace Kanji.Interface.Actors
{
    public class PipeActor
    {
        #region Static

        /// <summary>
        /// Creates and initializes the instance.
        /// </summary>
        public static void Initialize(NamedPipeHandler handler)
        {
            Instance = new PipeActor(handler, LogHelper.Factory.CreateLogger<PipeActor>());
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static PipeActor Instance { get; private set; }

        #endregion

        #region Fields

        private NamedPipeHandler _handler;
        private ILogger<PipeActor> _logger;

        #endregion

        #region Constructor

        private PipeActor(NamedPipeHandler handler, ILogger<PipeActor> logger)
        {
            _handler = handler;
            _handler.MessageReceived += OnMessageReceived;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the given message to a command, and executes the command.
        /// </summary>
        /// <param name="message">Message to process.</param>
        private void ProcessMessage(string message)
        {
            PipeMessageEnum? command = ParsingHelper.ParseEnum<PipeMessageEnum>(message);
            if (command.HasValue)
            {
                // Log the pipe command received.
                _logger.LogWarning(
                        "Received \"{value}\" pipe command.", command.Value);

                // Shutdown command.
                if (command.Value == PipeMessageEnum.OpenOrFocus)
                {
                    NavigationActor.Instance.OpenOrFocus();
                }
                else
                {
                    // Unhandled command. Ignore (but log).
                    _logger.LogWarning(
                        "Unhandled pipe command: \"{value}\". Ignoring.", command.Value);
                }
            }
        }

        #region Event callbacks

        /// <summary>
        /// Event callback.
        /// Called when a message is received by the associated pipe handler.
        /// </summary>
        private void OnMessageReceived(object sender, NamedPipeMessageEventArgs e)
        {
            ProcessMessage(e.Message);
        }

        #endregion

        #endregion
    }
}
