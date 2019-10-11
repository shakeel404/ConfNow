﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BackEnd.Data;
using ConferenceDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SearchController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchTerm term)
        {
            var query = term.Query;

            var sessionResults = await  _db.Sessions.AsNoTracking()
                                                   .Include(s => s.Track)
                                                   .Include(s => s.SessionSpeakers)
                                                     .ThenInclude(ss => ss.Speaker)
                                                   .Where(s =>
                                                       s.Title.Contains(query) ||
                                                       s.Track.Name.Contains(query)
                                                   )
                                                   .ToListAsync();

            var speakerResults =  await _db.Speakers.AsNoTracking()
                                                   .Include(s => s.SessionSpeakers)
                                                     .ThenInclude(ss => ss.Session)
                                                   .Where(s =>
                                                       s.Name.Contains(query) ||
                                                       s.Bio.Contains(query) ||
                                                       s.WebSite.Contains(query)
                                                   )
                                                   .ToListAsync();

             

            var results = sessionResults.Select(s => new SearchResult
            {
                Type = SearchResultType.Session,
                Value = JObject.FromObject(s.MapSessionResponse())
            })
            .Concat(speakerResults.Select(s => new SearchResult
            {
                Type = SearchResultType.Speaker,
                Value = JObject.FromObject(s.MapSpeakerResponse())
            }));

            return Ok(results.AsEnumerable());
        }
    }
}