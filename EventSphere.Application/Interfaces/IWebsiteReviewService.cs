using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;

namespace EventSphere.Application.Interfaces
{
    public interface IWebsiteReviewService
    {
        Task<List<WebsiteReview>> GetAllReviewsAsync();
        Task UpsertReviewAsync(WebsiteReview review);
    }
}
