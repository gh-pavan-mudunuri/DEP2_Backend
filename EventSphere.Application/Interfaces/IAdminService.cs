using Microsoft.AspNetCore.Mvc;

namespace backend.Interfaces
{
    public interface IAdminService
    {
        Task<IActionResult> AproveEventEditAsync(int id);
    }
}