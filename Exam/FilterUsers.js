function filterUserSkills(executionContext) {
    var formContext = executionContext.getFormContext();
    var skillLookup = formContext.getAttribute('test_skill');

    if (skillLookup && skillLookup.getValue() !== null) {
        var skillId = skillLookup.getValue()[0].id;
        var viewId = 'e88ca999-0b16-4ae9-b6a9-9edc840d42d8';
        var entity = 'systemuser';
        var viewDisplayName = 'User Lookup View';


        var fetchXML = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">' +
            '<entity name="systemuser">' +
            '<link-entity name="test_skills_systemuser" from="systemuserid" to="systemuserid" link-type="inner" intersect="true">' +
            '<filter type="and">' +
            '<condition attribute="test_skillid" operator="eq" uitype="test_skill" value="' + skillId + '" />' +
            '</filter>' +
            '</link-entity>' +
            '</entity>' +
            '</fetch>';

        var layout = '<grid name="resultset" jump="fullname" select="1" icon="1" preview="1">' +
            '<row name="result" id="systemuserid">' +
            '<cell name="fullname" width="300" />' +
            '</row>' +
            '</grid>';

        formContext.getControl('test_user').addCustomView(viewId, entity, viewDisplayName, fetchXML, layout, true);
    }
}
