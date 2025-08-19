using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Interfaces;
using EventSphere.Domain.Entities;
using EventSphere.Infrastructure.Repositories;

namespace EventSphere.Application.Services
{
    public class WebsiteReviewService : IWebsiteReviewService
    {
        private readonly WebsiteReviewRepository _repo;
        public WebsiteReviewService(WebsiteReviewRepository repo)
        {
            _repo = repo;
        }

        public Task<List<WebsiteReview>> GetAllReviewsAsync() => _repo.GetAllReviewsAsync();
        public Task UpsertReviewAsync(WebsiteReview review) => _repo.UpsertReviewAsync(review);
    }
}
