using Library.Database.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Library.Database.TableModels
{
    public class InterviewPrepQuestion
    {
        [Key]
        public string InterviewQuestionId { get; set; }
        public string Question { get; set; }
        public string Answer { get;set; }
        [ForeignKey("AddedById")]
        public SecureUser? AddedByUser { get; set; }
        public DateTime AddedOn { get; set; }
        [ForeignKey("LastEditById")]
        public SecureUser? LastEditByUser { get; set; }
        public DateTime? LastEditedOn { get; set; }

        public static void InsertOrUpdate(InterviewPrepQuestion interviewQuestion, ApplicationDbContext context, ClaimsPrincipal secureUser)
        {
            if (secureUser.IsInRole("God"))
            {
                InterviewPrepQuestion interviewQuestionMatch = context.InterviewPrepQuestion.FirstOrDefault(l => l.InterviewQuestionId == interviewQuestion.InterviewQuestionId);
                if (interviewQuestionMatch == null)
                {
                    interviewQuestionMatch = interviewQuestion;
                    context.Add<InterviewPrepQuestion>(interviewQuestionMatch);
                }
                else
                {
                    context.InterviewPrepQuestion.Attach(interviewQuestionMatch);
                    interviewQuestionMatch.Question = interviewQuestion.Question;
                    interviewQuestionMatch.Answer= interviewQuestion.Answer;
                    interviewQuestionMatch.LastEditByUser= interviewQuestionMatch.LastEditByUser;
                    interviewQuestionMatch.LastEditedOn = DateTime.UtcNow;
                }
                context.SaveChanges();
            }
        }

        public static InterviewPrepQuestion? GetById(string id, ApplicationDbContext context, ClaimsPrincipal secureUser)
        {
            InterviewPrepQuestion? interviewPrepQuestion = context.InterviewPrepQuestion.FirstOrDefault(l => l.InterviewQuestionId == id);
            return interviewPrepQuestion;
        }

        public static InterviewPrepQuestion? GetRandom(ApplicationDbContext context)
        {
            var questions = context.InterviewPrepQuestion.OrderBy(q => Guid.NewGuid()).Take(1);
            InterviewPrepQuestion interviewPrepQuestion = questions.FirstOrDefault<InterviewPrepQuestion>();
            return interviewPrepQuestion;
        }
    }
}
