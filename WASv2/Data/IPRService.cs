using System.Collections.Generic;
using WASv2.Models;

namespace WASv2.Data
{
    public interface IPRService
    {
        List<PRModel> GetPendingPRsForDepartmentHead(string department = null);
        PRModel GetPRByNumber(string prNumber);
        PRModel GetPRById(int id);
        PRModel CreatePR(PRModel prModel);
        bool ApprovePR(string prNumber, string reviewedBy, string remarks);
        bool DisapprovePR(string prNumber, string reviewedBy, string remarks);
        bool ForwardToProcurement(string prNumber);
        List<PRModel> GetPRsByStatus(string status, string department = null);
        List<PRModel> GetPRsByDepartment(string department);

        List<PRModel> GetPRsForDirector();
        bool ForwardToDirector(string prNumber);
        bool DirectorApprove(string prNumber, string reviewedBy, string remarks);
        bool DirectorReject(string prNumber, string reviewedBy, string remarks);
        bool DepartmentHeadApprovePR(string prNumber, string reviewedBy, string remarks);
    }

}