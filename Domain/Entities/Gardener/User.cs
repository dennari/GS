using Growthstories.Domain.Messaging;
using Growthstories.Domain.Interfaces;
/*
 * Copyright 2010-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Runtime.Serialization;

namespace Growthstories.Domain.Entities
{
    /// <summary>
    /// <para>The User data type contains information about a user.</para> <para> This data type is used as a response element in the following
    /// actions:</para>
    /// <ul>
    /// <li> <para> CreateUser </para> </li>
    /// <li> <para> GetUser </para> </li>
    /// <li> <para> ListUsers </para> </li>
    /// 
    /// </ul>
    /// </summary>
    public class User : AggregateBase<IEvent>
    {

        private string userName;
        private DateTime? createDate;

        /// <summary>
        /// The name identifying the user.
        ///  
        /// <para>
        /// <b>Constraints:</b>
        /// <list type="definition">
        ///     <item>
        ///         <term>Length</term>
        ///         <description>1 - 64</description>
        ///     </item>
        ///     <item>
        ///         <term>Pattern</term>
        ///         <description>[\w+=,.@-]*</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </summary>
        [Column]
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }




        /// <summary>
        /// The date when the user was created.
        ///  
        /// </summary>
        [Column]
        public DateTime? CreateDate
        {
            get { return this.createDate ?? default(DateTime); }
            set { this.createDate = value; }
        }

        public User(IEnumerable<IEvent> events)
            : base(events)
        {

        }


        public void ThrowOnInvalidStateTransition(ICommand<UserId> c)
        {
            if (Version == 0)
            {
                if (c is CreateUser)
                {
                    return;
                }
                throw DomainError.Named("premature", "Can't do anything to unexistent aggregate");
            }
            if (Version == -1)
            {
                throw DomainError.Named("zombie", "Can't do anything to deleted aggregate.");
            }
            if (c is CreateUser)
                throw DomainError.Named("rebirth", "Can't create aggregate that already exists");
        }
    }

    [DataContract]
    public class UserId : AbstractIdentity<Guid>
    {
        protected new string _tag = "user";

        public UserId(Guid Id) : base(Id) { }


    }

    public interface IUserHandler
    {
        void When(CreateUser c);
        //void When(ReportUserLoginFailure c);
        //void When(ReportUserLoginSuccess c);
        //void When(LockUser c);
        //void When(UnlockUser c);
        //void When(DeleteUser c);
    }

}
