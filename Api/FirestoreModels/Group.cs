﻿using Google.Cloud.Firestore;

namespace Api.Models
{
    [FirestoreData]
    public class Group : FirebaseDocument
    {
        [FirestoreProperty]
        public string Name { get; set; }
    }
}
