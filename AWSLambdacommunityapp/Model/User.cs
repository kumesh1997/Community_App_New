﻿using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class User
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool Is_Super_Admin { get; set; } = false;
        public string Phone_Number { get; set; }
        public bool Email_Verified { get; set; } = false;

        public Dictionary<string, bool> AdditionalAttributes = new Dictionary<string, bool>();

        //Condominium_Id, Module_Id
    }
}
