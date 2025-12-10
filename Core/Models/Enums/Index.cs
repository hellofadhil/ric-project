namespace Core.Models.Enums
{
    public enum Role
    {
        User_Member,
        User_Pic,
        User_Manager,
        User_VP,
        BR_Pic,
        BR_Member,
        BR_Manager,
        BR_VP,
        SARM_Pic,
        SARM_Member,
        SARM_Manager,
        SARM_VP,
        ECS_Pic,
        ECS_Member,
        ECS_Manager,
        ECS_VP,
    }

    public enum RoleApproval
    {
        User_Manager,
        User_VP,
        BR_Manager,
        SARM_Manager,
        SARM_VP,
        ECS_Manager,
        ECS_VP,
    }

    public enum StatusRic
    {
        Draft,
        Submitted_To_BR,
        Review_BR,
        Return_BR_To_User,
        Review_SARM,
        Review_ECS,
        Return_SARM_To_BR,
        Return_ECS_To_BR,
        Approval_Manager_User,
        Approval_VP_User,
        Approval_Manager_BR,
        Approval_Manager_SARM,
        Approval_VP_SARM,
        Approval_Manager_ECS,
        Approval_VP_ECS,
        Done,
    }

    public enum ApprovalStatus
    {
        Pending,
        Approved,
    }

    public enum RoleReview
    {
        BR,
        SARM,
        ECS,
    }
}
