﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Capstone.Container_Classes;
using Capstone.Data;
using Capstone.Core;
using Capstone.Models;

//return _Map(_repository.GetAll<Data.Resource>());
//return _Map(_repository.GetAll<Data.Resource>(resource => resource.ResourceType == typeId));
//return _Map(_repository.Get<Data.Resource>(resource => resource.ResourceId == resourceId));

namespace Capstone.Managers
{
    public class UserManager
    {

        private static Entities2 _entities = new Entities2();
        private IRepository _repository = new Repository(_entities);

        // Once the controller has passed the User info and foods list it can be inserted into the database
        // and a view of that inserted user is returned. 
        public UserViewModels CreateUser(Container_Classes.User NewUser)
        {
            // Ensure that the User does not already exist in the database.
            if (_repository.Get<Data.User>(x => x.Username == NewUser.Username) == null)
            {
                // The user is in the database, this should be a special error. 
                return null;
            }
            // Add the new user to the database.
            Data.User dUser = Container_Classes.User.UserToDataUser(NewUser);
            _repository.Add<Data.User>(dUser);

            // Get the inserted NewUser back from the database
            dUser =_repository.Get<Data.User>(x => x.Username == NewUser.Username);
            if (dUser == null)
            {
                // The user was not uploaded into the database.
                return null;
            }

            // Inserting the foods into the seperate table
            foreach (Container_Classes.Food food in NewUser.Foods)
            {
                Data.Food dFood = new Data.Food();
                dFood.Food1 = food.FoodString;
                dFood.User_ID = dUser.Id;
                _repository.Add<Data.Food>(dFood);
            }

            _repository.SaveChanges();

            // Get the foods from the database to give back to the view
            List<Data.Food> dbFoods = DatabaseToDataFoods(_repository.GetAll<Data.Food>(x => x.User_ID == dUser.Id));
            List<Container_Classes.Food> containerFoods = Container_Classes.Food.DataFoodsToContainerFoods(dbFoods);

            // Create the model to return the information to the view.
            UserViewModels model = new UserViewModels();
            model.User = Container_Classes.User.DataUserToUser(dUser, containerFoods);

            return model;
        }

        public UserViewModels UpdateUser(Container_Classes.User UpdatedUser)
        {
            //Attempt to find the user from the database before sending update.
            if (_repository.Get<Container_Classes.User>(x => x.Username == UpdatedUser.Username) == null)
            {
                // The user was not found in the database
                return null;
            }

            Data.User dUser = Container_Classes.User.UserToDataUser(UpdatedUser);
            _repository.Update<Data.User>(dUser);

            Data.User addedDataUser = _repository.Get<Data.User>(x => x.Username == UpdatedUser.Username);
            if (addedDataUser == null)
            {
                // The user should have been inserted into the database, but they were not. (Or failed during insertion)
                return null;
            }

            // Clear the user's food if there are any
            List<Data.Food> dbFoods = DatabaseToDataFoods(_repository.GetAll<Data.Food>(x => x.User_ID == addedDataUser.Id));
            foreach (Data.Food dataFood in dbFoods)
            {
                _repository.Delete<Data.Food>(dataFood);
            }

            // Insert the user's foods if they have any
            dbFoods = Container_Classes.Food.ContainerFoodsToDataFoods(UpdatedUser.Foods);
            foreach (Data.Food dataFood in dbFoods)
            {
                // Tag the ID with the foods to be inserted
                dataFood.User_ID = addedDataUser.Id;
                // Insert the foods
                _repository.Add<Data.Food>(dataFood);
            }

            _repository.SaveChanges();

            UpdatedUser = Container_Classes.User.DataUserToUser(addedDataUser, UpdatedUser.Foods);

            // Create the model to return the information to the view.
            UserViewModels model = new UserViewModels();
            model.User = UpdatedUser;

            return model;
        }

        public UserViewModels GetUserByID(int UserID)
        {
            // Attempt to find the user through the requested username.
            Data.User dUser = _repository.Get<Data.User>(x => x.Id == UserID);
            // Ensure that we actually found someone through the username.
            if (dUser == null)
            {
                // We did not find anyone by the provided username in the database.
                return null;
            }
            // Get the foods from the database associated with the user
            List<Data.Food> dataFoods = DatabaseToDataFoods(_repository.GetAll<Data.Food>(x => x.User_ID == UserID));
            List<Container_Classes.Food> containerFoods = Container_Classes.Food.DataFoodsToContainerFoods(dataFoods);

            // Combine the foods and user information for the database for the user object
            Container_Classes.User containerUser = Container_Classes.User.DataUserToUser(dUser, containerFoods);

            // Create the model to return the information to the view.
            UserViewModels model = new UserViewModels();
            model.User = containerUser;

            return model;
        }

        // This cannot be implemented until the Events have been added to the database entities.
        public UsersViewModels GetUsersByEventID(int EventID)
        {
            // Find the list of users attending the event
            List<Data.Registration> dataRegistrations = DatabaseToDataRegistration(_repository.GetAll<Data.Registration>(x => x.Event_ID == EventID));
            
            // Create the List of Data.User id's attending the event
            List<int> dataUsersID = new List<int>();

            foreach (Data.Registration dataRegistration in dataRegistrations)
            {
                dataUsersID.Add(dataRegistration.User_ID);
            }

            // Retieve these users from the database and read into List of Data.User
            List<Data.User> dataUsers = new List<Data.User>();

            foreach (int userID in dataUsersID)
            {
                dataUsers.Add(_repository.Get<Data.User>(x => x.Id == userID));
            }

            // Convert the data.users to usable user objects
            List<Container_Classes.User> containerUsers = new List<Container_Classes.User>();
            foreach (Data.User dataUser in dataUsers)
            {
                containerUsers.Add(Container_Classes.User.DataUserToUser(dataUser));
            }

            UsersViewModels model = new UsersViewModels();
            model.Users = containerUsers;

            return model;
        }

        public List<Data.Food> DatabaseToDataFoods(IEnumerable<Data.Food> source)
        {
            List<Data.Food> dataFoods = new List<Data.Food>();

            foreach (Data.Food dbFood in source)
            {
                dataFoods.Add(dbFood);
            }

            return dataFoods;
        }

        public List<Data.Registration> DatabaseToDataRegistration(IEnumerable<Data.Registration> source)
        {
            List<Data.Registration> dataRegistrations = new List<Data.Registration>();

            foreach (Data.Registration dbRegistration in source)
            {
                dataRegistrations.Add(dbRegistration);
            }

            return dataRegistrations;
        }

        
    }
}