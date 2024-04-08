function filterUserSkills(executionContext) {
    var formContext = executionContext.getFormContext();
    var skillLookup = formContext.getAttribute("test_skill");

    if (skillLookup && skillLookup.getValue() !== null) {
        var skillId = skillLookup.getValue()[0].id;
        var viewId = "E88CA999-0B16-4AE9-B6A9-9EDC840D42D8";
        var entity = "systemuser";
        var viewDisplayName = "User Lookup View";


        var fetchXML = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
            "<entity name='systemuser'>" +
            "<link-entity name='test_skills_systemuser' from='systemuserid' to='systemuserid' link-type='inner' alias='aa' intersect='true'>" +
            "<filter type='and'>" +
            "<condition attribute='test_skillsid' operator='eq' uitype='test_skill' value='" + skillId + "' />" +
            "</filter>" +
            "</link-entity>" +
            "</entity>" +
            "</fetch>";

        var layout = "<grid name='resultset' jump='fullname' select='1' icon='1' preview='1'>" +
            "<row name='result' id='contactid'>" +
            "<cell name='fullname' width='300' />" +
            "</row>" +
            "</grid>";

        formContext.getControl("test_user").addCustomView(viewId, entity, viewDisplayName, fetchXML, layout, true);
    }
}