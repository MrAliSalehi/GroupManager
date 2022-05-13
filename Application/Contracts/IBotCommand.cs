using GroupManager.DataLayer.Models;

namespace GroupManager.Application.Contracts;

public interface IBotCommand
{
    public Group? CurrentGroup { get; set; }
    // public Group? GetGroup() => CurrentGroup;
}