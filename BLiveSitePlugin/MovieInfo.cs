using System;
namespace BLiveSitePlugin
{
    class MovieInfo
    {
        public long MovieId { get; set; }
        public long OnairStatus { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }
        public MovieInfo(Low.External.Movies.RootObject obj)
        {
            MovieId = obj.MovieId;
            OnairStatus=obj.OnairStatus;
            StartedAt = obj.StartedAt;
            Title = obj.Title;
            Id = obj.Id;
        }
        public MovieInfo()
        {
        }
    }
}
