﻿using CareerPortal.DataAccess.Data;
using CareerPortal.DataAccess.Repository;
using CareerPortal.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            Education = new EducationRepository(_db);
            Experience = new ExperienceRepository(_db);
            User = new UserRepository(_db);
        }

        


        public IUserRepository User { get; private set; }
        public IEducationRepository Education { get; private set; }
        public IExperienceRepository Experience { get; private set; }



        public void Dispose()
        {
            _db.Dispose();
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
