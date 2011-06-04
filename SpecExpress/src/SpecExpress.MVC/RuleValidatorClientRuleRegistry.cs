﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SpecExpress.Rules;
using SpecExpress.Rules.GeneralValidators;
using SpecExpress.Rules.StringValidators;

namespace SpecExpress.MVC
{
    class RuleValidatorClientRuleMap
    {
        public RuleValidatorClientRuleMap()
        {
            Parameters = new Dictionary<string, int>();
        }

        public string JQueryRuleName { get; set; }
        public Dictionary<string, int > Parameters { get; set; }
        
    }
    public sealed class RuleValidatorClientRuleRegistry
    {
        static readonly RuleValidatorClientRuleRegistry instance = new RuleValidatorClientRuleRegistry();
        static Dictionary<Type, RuleValidatorClientRuleMap> Mapping;
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static RuleValidatorClientRuleRegistry()
        {
        }

        RuleValidatorClientRuleRegistry()
        {
            Mapping = new Dictionary<Type, RuleValidatorClientRuleMap>();
            
            //Required
            Mapping.Add(typeof(Required<,>), new RuleValidatorClientRuleMap() { JQueryRuleName = "specrequired"});

            //MinLength
            var minLength = new RuleValidatorClientRuleMap();
            minLength.JQueryRuleName = "specminlength";
            minLength.Parameters.Add("length",0);
            Mapping.Add(typeof(MinLength<>), minLength);

            //MaxLength
            var maxLength = new RuleValidatorClientRuleMap();
            maxLength.JQueryRuleName = "specmaxlength";
            maxLength.Parameters.Add("length", 0);
            Mapping.Add(typeof(MaxLength<>), maxLength);

            //TODO: Add More Mappings HERE, THEN in specexpress.ubobtrusive.js


        }

        public static RuleValidatorClientRuleRegistry Instance
        {
            get
            {
                return instance;
            }
        }

        public ModelClientValidationRule Create(RuleValidator ruleValidator)
        {
            var clientRule = new ModelClientValidationRule();

            var ruleValidatorType = ruleValidator.GetType().GetGenericTypeDefinition();
            if (!Mapping.ContainsKey(ruleValidatorType))
            {
                return null;
            }

            var rule = Mapping[ruleValidatorType];

            clientRule.ValidationType = rule.JQueryRuleName;
            clientRule.ErrorMessage = ruleValidator.ErrorMessageTemplate;

            //map all the parameters
            foreach (var parameter in rule.Parameters )
            {
                //parameter.value is the index of the matching value in the rulevalidator parameters collection
                clientRule.ValidationParameters.Add(parameter.Key, ruleValidator.Parameters[parameter.Value]);
            }

            return clientRule;
        }


    }

}