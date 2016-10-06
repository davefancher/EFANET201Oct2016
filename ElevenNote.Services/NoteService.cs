using System;
using System.Collections.Generic;
using System.Linq;
using ElevenNote.Data;
using ElevenNote.Models;

namespace ElevenNote.Services
{
    public class NoteService
    {
        public static U CopyProperties<T, U>(T source)
            where U : new()
        {
            var sourceType = typeof(T);
            var targetType = typeof(U);

            return
                targetType
                    .GetProperties()
                    .Aggregate(
                        new U(),
                        (target, pi) =>
                        {
                            var value = sourceType.GetProperty(pi.Name)?.GetValue(source);
                            if (value != null) pi.SetValue(target, value);
                            return target;
                        }
                    );
        }

        private readonly Guid _userId;

        public NoteService(Guid userId)
        {
            _userId = userId;
        }

        public IEnumerable<NoteListItemModel> GetNotes()
        {
            using (var context = new ApplicationDbContext())
            {
                return
                    context
                        .Notes
                        .Where(n => n.OwnerId == _userId)
                        .Select(CopyProperties<NoteEntity, NoteListItemModel>)
                        .ToArray();
            }
        }

        private static NoteEntity GetNoteById(
            ApplicationDbContext context,
            int noteId,
            Guid userId) =>
                context
                    .Notes
                    .SingleOrDefault(n => n.NoteId == noteId && n.OwnerId == userId);

        public NoteDetailModel GetNoteById(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                var entity = GetNoteById(context, id, _userId);

                return
                    entity == null
                    ? new NoteDetailModel()
                    : CopyProperties<NoteEntity, NoteDetailModel>(entity);
            }
        }

        public bool CreateNote(NoteCreateModel model)
        {
            using (var context = new ApplicationDbContext())
            {
                var entity =
                    CopyProperties<NoteCreateModel, NoteEntity>(model);
                entity.OwnerId = _userId;
                entity.CreatedUtc = DateTimeOffset.UtcNow;

                context.Notes.Add(entity);

                return context.SaveChanges() == 1;
            }
        }

        public bool UpdateNote(NoteEditModel model)
        {
            using (var context = new ApplicationDbContext())
            {
                var entity = GetNoteById(context, model.NoteId, _userId);

                // TODO: Handle Note not found

                entity.Title = model.Title;
                entity.Content = model.Content;
                entity.ModifiedUtc = DateTimeOffset.UtcNow;

                return context.SaveChanges() == 1;
            }
        }

        public bool DeleteNote(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                var entity = GetNoteById(context, id, _userId);

                // TODO: Handle note not found

                context.Notes.Remove(entity);

                return context.SaveChanges() == 1;
            }
        }
    }
}
