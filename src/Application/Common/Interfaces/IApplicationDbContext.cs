using Microsoft.EntityFrameworkCore;
using TwitterCloneApi.Domain.Entities;

namespace TwitterCloneApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
