using GroupManager.DataLayer.Models;

namespace GroupManager.Application.Contracts;

public interface IUserInvolvedCommand
{
    public User? User { get; set; }
}