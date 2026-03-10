using System.Collections.Generic;

namespace FantasyLeagueManager.DataAccess.Interfaces
{
    public interface IDataAccess<T>
    {
        // CRUD methods
        List<T> GetAll();
        T GetById(int id);
        void Add(T item);
        void Update(T item);
        void Delete(int id);

        // Filter players by position 
        List<T> GetByPosition(string position);
    }
}