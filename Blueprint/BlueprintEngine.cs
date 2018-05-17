using System;
using System.Collections.Generic;
using System.Linq;
using OrderManSys.Model;
using OrderManSys.Repository;

namespace OrderManSys.Blueprint
{
    //This is the Analyze engine's code.
    //Engine will accept a Product type and return all the instruction at once. 
    //(If the production's instruction are registered in the database.)
    public class BlueprintEngine
    {
        private readonly InstructionRepo instructionRepo;
        public BlueprintEngine()
        {
            instructionRepo = new InstructionRepo();
        }

        public IEnumerable<Instruction> GetRegistred(Product product)
        {
            var parameters = new Dictionary<string,object>();
            parameters.Add("product",1);
            return(instructionRepo.Get(parameters));
        }
    }
}