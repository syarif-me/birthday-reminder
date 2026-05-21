namespace BirthdayReminder.Domain.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(Guid userId) 
        : base($"User with ID {userId} was not found.")
    {
    }
}
