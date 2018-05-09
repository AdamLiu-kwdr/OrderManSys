using System;
using System.Collections.Generic;

namespace OrderManSys.Repository
{
    //This interface defines what's required for every database repository in the system.
    public interface IDBRepository<T>
    {
         //Get a single instance by id
         T GetbyId(int Id);
         //Get all instances (Will be deleted in future!)
         IEnumerable<T> GetAll();
         //Get instances with querry parameters (see description below.)
         IEnumerable<T> Get(Dictionary<string,object> WhereParameters);
         void Create(T entity);
         void Delete(int id);
         void Update(T entity);
    }
}

/* 
Get(WhereParameters) method should querry the database with dynamically selected column and parameters.
When calling this function, Caller should create an dictionary contains <Column name string,Querry value object> as Querry Parameters.
        
For example:
var Parameters = new Dictionary<string,object>();
Parameters.Add("column name",value);

This is a work around cause' I don't know how to map LINQ to dapper ;w;
*/