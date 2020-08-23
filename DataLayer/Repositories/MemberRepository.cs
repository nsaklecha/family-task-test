using Core.Abstractions.Repositories;
using Domain.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLayer
{
    public class MemberRepository : BaseRepository<Guid, Member, MemberRepository>, IMemberRepository
    {
        protected readonly FamilyTaskContext MemberContext;
        protected IQueryable<Member> MemberQuery;
        public MemberRepository(FamilyTaskContext context) : base(context)
        {
            MemberContext = context;
            MemberQuery = context.Set<Member>();
        }

       

        IMemberRepository IBaseRepository<Guid, Member, IMemberRepository>.NoTrack()
        {
            return base.NoTrack();
        }

        IMemberRepository IBaseRepository<Guid, Member, IMemberRepository>.Reset()
        {
            return base.Reset();
        }

       
    }
}
