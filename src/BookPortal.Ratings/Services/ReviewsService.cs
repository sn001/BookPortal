﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookPortal.Core.Framework.Models;
using BookPortal.Ratings.Domain;
using BookPortal.Ratings.Models;
using BookPortal.Ratings.Models.Shims;
using BookPortal.Ratings.Models.Types;
using Microsoft.Data.Entity;

namespace BookPortal.Ratings.Services
{
    public class ReviewsService
    {
        private readonly RatingsContext _ratingsContext;

        public ReviewsService(RatingsContext ratingsContext)
        {
            _ratingsContext = ratingsContext;
        }

        public async Task<ApiObject<ReviewResponse>> GetReviewsAsync(List<int> workIds, int userId)
        {
            var queryResult = await _ratingsContext.Reviews
                .Where(c => workIds.Contains(c.WorkId))
                .Select(c => new ReviewResponse
                {
                    Id = c.Id,
                    Text = c.Text,
                    WorkId = c.WorkId,
                    DateCreated = c.DateCreated,
                    UserId = c.UserId,
                }).ToListAsync();

            var result = new ApiObject<ReviewResponse>();
            result.Values = queryResult;
            result.TotalRows = result.Values.Count;

            return result;
        }

        public async Task<ReviewResponse> GetReviewAsync(int reviewId)
        {
            var queryResult = await _ratingsContext.Reviews
                .Where(c => c.Id == reviewId)
                .Select(c => new ReviewResponse
                {
                    Id = c.Id,
                    Text = c.Text,
                    WorkId = c.WorkId,
                    DateCreated = c.DateCreated,
                    UserId = c.UserId,
                }).SingleOrDefaultAsync();

            // get review vote
            queryResult.ReviewRating = await _ratingsContext.ReviewVotes
                .Where(c => c.ReviewId == reviewId)
                .SumAsync(c => c.Vote);

            // get user names
            var user = GetUsers(queryResult.UserId).SingleOrDefault();

            // get work names
            var work = GetWorks(queryResult.WorkId).SingleOrDefault();

            queryResult.WorkName = work?.Name;
            queryResult.WorkRusname = work?.RusName;
            queryResult.UserName = user?.Name;

            return queryResult;
        }

        // TODO: Move to main service
        public async Task<IReadOnlyList<ReviewResponse>> GetReviewsPersonAsync(ReviewPersonRequest reviewRequest)
        {
            if (reviewRequest == null)
            {
                throw new ArgumentNullException(nameof(reviewRequest));
            }

            var workIds = GetPersonWorks(reviewRequest.PersonId);

            var query = _ratingsContext.Reviews.Where(c => workIds.Contains(c.WorkId));

            switch (reviewRequest.Sort)
            {
                case ReviewSort.Date:
                    query = query.OrderByDescending(c => c.DateCreated);
                    break;
                case ReviewSort.Rating:
                    query = query.OrderByDescending(c => c.DateCreated);
                    break;
            }

            if (reviewRequest.Offset.HasValue && reviewRequest.Offset.Value > 0)
                query = query.Skip(reviewRequest.Offset.Value);

            if (reviewRequest.Limit.HasValue && reviewRequest.Limit > 0)
                query = query.Take(reviewRequest.Limit.Value);

            var queryResult = await query.Select(c => new ReviewResponse
            {
                Id = c.Id,
                Text = c.Text,
                WorkId = c.WorkId,
                DateCreated = c.DateCreated,
                UserId = c.UserId,
            }).ToListAsync();

            // get response votes
            var reviewIds = queryResult.GroupBy(c => c.Id).Select(grp => grp.Key).ToList();
            var reviewVotes = (from c in _ratingsContext.ReviewVotes
                               where reviewIds.Contains(c.ReviewId)
                               group c by c.ReviewId into g
                               select new
                               {
                                   ReviewId = g.Key,
                                   ReviewRating = g.Sum(c => c.Vote)
                               }).ToList();

            // get user names
            var userIds = queryResult.GroupBy(c => c.UserId).Select(grp => grp.Key).ToArray();
            var users = GetUsers(userIds);

            // get work names
            var works = GetWorks(workIds.ToArray());

            foreach (var result in queryResult)
            {
                WorkResponseShim workResponseShim = works.SingleOrDefault(c => c.WorkId == result.WorkId);
                result.WorkName = workResponseShim?.Name;
                result.WorkRusname = workResponseShim?.RusName;
                result.UserName =
                    users.Where(c => c.UserId == result.UserId).Select(c => c.Name).SingleOrDefault();
                result.ReviewRating =
                    reviewVotes.Where(c => c.ReviewId == result.Id).Select(c => c.ReviewRating).SingleOrDefault();
            }

            return queryResult;
        }

        // TODO: Move to main service
        public async Task<IReadOnlyList<ReviewResponse>> GetReviewsWorkAsync(ReviewWorkRequest reviewRequest)
        {
            if (reviewRequest == null)
            {
                throw new ArgumentNullException(nameof(reviewRequest));
            }

            var query = _ratingsContext.Reviews.Where(c => c.WorkId == reviewRequest.WorkId);

            switch (reviewRequest.Sort)
            {
                case ReviewSort.Date:
                    query = query.OrderByDescending(c => c.DateCreated);
                    break;
                case ReviewSort.Rating:
                    query = query.OrderByDescending(c => c.DateCreated);
                    break;
            }

            if (reviewRequest.Offset.HasValue && reviewRequest.Offset.Value > 0)
                query = query.Skip(reviewRequest.Offset.Value);

            if (reviewRequest.Limit.HasValue && reviewRequest.Limit > 0)
                query = query.Take(reviewRequest.Limit.Value);

            var queryResult = await query.Select(c => new ReviewResponse
            {
                Id = c.Id,
                Text = c.Text,
                WorkId = c.WorkId,
                DateCreated = c.DateCreated,
                UserId = c.UserId,
            }).ToListAsync();

            // get response votes
            var reviewIds = queryResult.GroupBy(c => c.Id).Select(grp => grp.Key).ToList();
            var reviewVotes = ( from c in _ratingsContext.ReviewVotes
                                where reviewIds.Contains(c.ReviewId)
                                group c by c.ReviewId into g
                                select new
                                {
                                    ReviewId = g.Key,
                                    ReviewRating = g.Sum(c => c.Vote)
                                }).ToList();

            // get user names
            var userIds = queryResult.GroupBy(c => c.UserId).Select(grp => grp.Key).ToArray();
            var users = GetUsers(userIds);

            // get work names
            var works = GetWorks(reviewRequest.WorkId);

            foreach (var result in queryResult)
            {
                WorkResponseShim workResponseShim = works.SingleOrDefault(c => c.WorkId == result.WorkId);
                result.WorkName = workResponseShim?.Name;
                result.WorkRusname = workResponseShim?.RusName;
                result.UserName = 
                    users.Where(c => c.UserId == result.UserId).Select(c => c.Name).SingleOrDefault();
                result.ReviewRating =
                    reviewVotes.Where(c => c.ReviewId == result.Id).Select(c => c.ReviewRating).SingleOrDefault();
            }

            return queryResult;
        }

        // TODO: Get user rating for this work (from BookPortal.Rating service)
        public async Task<IReadOnlyList<ReviewResponse>> GetReviewsUserAsync(ReviewUserRequest reviewRequest)
        {
            if (reviewRequest == null)
            {
                throw new ArgumentNullException(nameof(reviewRequest));
            }

            var query = _ratingsContext.Reviews.Where(c => c.UserId == reviewRequest.UserId);

            switch (reviewRequest.Sort)
            {
                case ReviewSort.Date:
                    query = query.OrderByDescending(c => c.DateCreated);
                    break;
                case ReviewSort.Rating:
                    query = query.OrderByDescending(c => c.DateCreated);
                    break;
            }

            if (reviewRequest.Offset.HasValue && reviewRequest.Offset.Value > 0)
                query = query.Skip(reviewRequest.Offset.Value);

            if (reviewRequest.Limit.HasValue && reviewRequest.Limit > 0)
                query = query.Take(reviewRequest.Limit.Value);

            var queryResult = await query.Select(c => new ReviewResponse
            {
                Id = c.Id,
                Text = c.Text,
                WorkId = c.WorkId,
                DateCreated = c.DateCreated,
                UserId = c.UserId,
            }).ToListAsync();

            // get response votes
            var reviewIds = queryResult.GroupBy(c => c.Id).Select(grp => grp.Key).ToList();
            var reviewVotes = (from c in _ratingsContext.ReviewVotes
                               where reviewIds.Contains(c.ReviewId)
                               group c by c.ReviewId into g
                               select new
                               {
                                   ReviewId = g.Key,
                                   ReviewRating = g.Sum(c => c.Vote)
                               }).ToList();

            // get user names
            var users = GetUsers(reviewRequest.UserId);

            // get work names
            var workIds = queryResult.GroupBy(c => c.WorkId).Select(grp => grp.Key).ToArray();
            var works = GetWorks(workIds);

            foreach (var result in queryResult)
            {
                WorkResponseShim workResponseShim = works.SingleOrDefault(c => c.WorkId == result.WorkId);
                result.WorkName = workResponseShim?.Name;
                result.WorkRusname = workResponseShim?.RusName;
                result.UserName =
                    users.Where(c => c.UserId == result.UserId).Select(c => c.Name).SingleOrDefault();
                result.ReviewRating =
                    reviewVotes.Where(c => c.ReviewId == result.Id).Select(c => c.ReviewRating).SingleOrDefault();
            }

            return queryResult;
        }

        // TODO: Get from BookPortal.Auth service
        private IReadOnlyList<UserResponseShim> GetUsers(params int[] userIds)
        {
            var list = new List<UserResponseShim>(userIds.Length);

            foreach (var userId in userIds)
            {
                list.Add(new UserResponseShim { UserId = userId, Name = "ravenger" });
            }

            return list;
        }

        // TODO: Get from BookPortal.Web service
        private IReadOnlyList<int> GetPersonWorks(int personId)
        {
            var list = new List<int>();

            list.Add(1);

            return list;
        }

        // TODO: Get from BookPortal.Web service
        private IReadOnlyList<WorkResponseShim> GetWorks(params int[] workIds)
        {
            var list = new List<WorkResponseShim>(workIds.Length);

            foreach (var workId in workIds)
            {
                list.Add(new WorkResponseShim { WorkId = workId, RusName = "Гиперион", Name = "Hyperion" });
            }

            return list;
        }
    }
}