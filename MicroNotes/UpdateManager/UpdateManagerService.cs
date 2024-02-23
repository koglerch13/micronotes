using System;
using System.Threading.Tasks;
using MicroNotes.MessageBox;
using Microsoft.Extensions.Logging;
using Velopack.Sources;

namespace MicroNotes.UpdateManager;

public class UpdateManagerService : IUpdateManagerService
{
    private readonly ILogger _logger;
    private readonly IMessageBoxService _messageBoxService;

    public UpdateManagerService(ILogger logger, IMessageBoxService messageBoxService)
    {
        _logger = logger;
        _messageBoxService = messageBoxService;
    }
    
    public async Task TryUpdate()
    {
        try
        {
            _logger.LogInformation("Check for updates.");
            
            var updateManager = new Velopack.UpdateManager(new GithubSource(Constants.UPDATE_URL, null, false));
            
            var newVersion = await updateManager.CheckForUpdatesAsync();
            if (newVersion == null)
            {
                // nothing to update.
                _logger.LogInformation("No new version.");
                return; 
            }
            
            _logger.LogInformation("Update available.");
            var doUpdate = await _messageBoxService.AskForUpdate();
            if (doUpdate)
            {
                _logger.LogInformation("Try to update.");
                await updateManager.DownloadUpdatesAsync(newVersion);
                updateManager.ApplyUpdatesAndRestart(newVersion);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating.");
        }
    }
}